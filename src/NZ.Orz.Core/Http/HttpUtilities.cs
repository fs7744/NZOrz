using NZ.Orz.Infrastructure;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace NZ.Orz.Http;

public static class HttpUtilities
{
    #region HttpCharacters

    // ALPHA and DIGIT https://tools.ietf.org/html/rfc5234#appendix-B.1
    private const string AlphaNumeric = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    // Authority https://tools.ietf.org/html/rfc3986#section-3.2
    // Examples:
    // microsoft.com
    // hostname:8080
    // [::]:8080
    // [fe80::]
    // 127.0.0.1
    // user@host.com
    // user:password@host.com
    private static readonly SearchValues<byte> _allowedAuthorityBytes = SearchValues.Create(":.-[]@0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"u8);

    // Matches Http.Sys
    // Matches RFC 3986 except "*" / "+" / "," / ";" / "=" and "%" HEXDIG HEXDIG which are not allowed by Http.Sys
    private static readonly SearchValues<char> _allowedHostChars = SearchValues.Create("!$&'()-._~" + AlphaNumeric);

    // tchar https://tools.ietf.org/html/rfc7230#appendix-B
    private static readonly SearchValues<char> _allowedTokenChars = SearchValues.Create("!#$%&'*+-.^_`|~" + AlphaNumeric);

    private static readonly SearchValues<byte> _allowedTokenBytes = SearchValues.Create("!#$%&'*+-.^_`|~0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"u8);

    // field-value https://tools.ietf.org/html/rfc7230#section-3.2
    // HTAB, [VCHAR, SP]
    private static readonly SearchValues<char> _allowedFieldChars = SearchValues.Create("\t !\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~" + AlphaNumeric);

    // Values are [0x00, 0x1F] without 0x09 (HTAB) and with 0x7F.
    private static readonly SearchValues<char> _invalidFieldChars = SearchValues.Create(
        "\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\u0008\u000A\u000B\u000C\u000D\u000E\u000F\u0010" +
        "\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001A\u001B\u001C\u001D\u001E\u001F\u007F");

    public static bool ContainsInvalidAuthorityChar(ReadOnlySpan<byte> span) => span.IndexOfAnyExcept(_allowedAuthorityBytes) >= 0;

    public static int IndexOfInvalidHostChar(ReadOnlySpan<char> span) => span.IndexOfAnyExcept(_allowedHostChars);

    public static int IndexOfInvalidTokenChar(ReadOnlySpan<char> span) => span.IndexOfAnyExcept(_allowedTokenChars);

    public static int IndexOfInvalidTokenChar(ReadOnlySpan<byte> span) => span.IndexOfAnyExcept(_allowedTokenBytes);

    // Follows field-value rules in https://tools.ietf.org/html/rfc7230#section-3.2
    // Disallows characters > 0x7E.
    public static int IndexOfInvalidFieldValueChar(ReadOnlySpan<char> span) => span.IndexOfAnyExcept(_allowedFieldChars);

    // Follows field-value rules for chars <= 0x7F. Allows extended characters > 0x7F.
    public static int IndexOfInvalidFieldValueCharExtended(ReadOnlySpan<char> span) => span.IndexOfAny(_invalidFieldChars);

    #endregion HttpCharacters

    public const string HttpUriScheme = "http://";
    public const string HttpsUriScheme = "https://";

    private static readonly ulong _httpSchemeLong = GetAsciiStringAsLong(HttpUriScheme + "\0");
    private static readonly ulong _httpsSchemeLong = GetAsciiStringAsLong(HttpsUriScheme);

    private const uint _httpGetMethodInt = 542393671; // GetAsciiStringAsInt("GET ");
    private const ulong _http10VersionLong = 3471766442030158920; // GetAsciiStringAsLong("HTTP/1.0");
    private const ulong _http11VersionLong = 3543824036068086856; // GetAsciiStringAsLong("HTTP/1.1");

    private static readonly UTF8Encoding DefaultRequestHeaderEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    private static readonly ulong _httpConnectMethodLong = GetAsciiStringAsLong("CONNECT ");
    private static readonly ulong _httpDeleteMethodLong = GetAsciiStringAsLong("DELETE \0");
    private static readonly ulong _httpHeadMethodLong = GetAsciiStringAsLong("HEAD \0\0\0");
    private static readonly ulong _httpPatchMethodLong = GetAsciiStringAsLong("PATCH \0\0");
    private static readonly ulong _httpPostMethodLong = GetAsciiStringAsLong("POST \0\0\0");
    private static readonly ulong _httpPutMethodLong = GetAsciiStringAsLong("PUT \0\0\0\0");
    private static readonly ulong _httpOptionsMethodLong = GetAsciiStringAsLong("OPTIONS ");
    private static readonly ulong _httpTraceMethodLong = GetAsciiStringAsLong("TRACE \0\0");

    private static readonly ulong _mask8Chars = GetMaskAsLong([0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff]);

    private static readonly ulong _mask7Chars = GetMaskAsLong([0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x00]);

    private static readonly ulong _mask6Chars = GetMaskAsLong([0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x00, 0x00]);

    private static readonly ulong _mask5Chars = GetMaskAsLong([0xff, 0xff, 0xff, 0xff, 0xff, 0x00, 0x00, 0x00]);

    private static readonly ulong _mask4Chars = GetMaskAsLong([0xff, 0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00]);

    private static readonly Tuple<ulong, ulong, HttpMethod, int>[] _knownMethods =
            new Tuple<ulong, ulong, HttpMethod, int>[17];

    private static readonly string[] _methodNames = new string[9];

    static HttpUtilities()
    {
        SetKnownMethod(_mask4Chars, _httpPutMethodLong, HttpMethod.Put, 3);
        SetKnownMethod(_mask5Chars, _httpHeadMethodLong, HttpMethod.Head, 4);
        SetKnownMethod(_mask5Chars, _httpPostMethodLong, HttpMethod.Post, 4);
        SetKnownMethod(_mask6Chars, _httpPatchMethodLong, HttpMethod.Patch, 5);
        SetKnownMethod(_mask6Chars, _httpTraceMethodLong, HttpMethod.Trace, 5);
        SetKnownMethod(_mask7Chars, _httpDeleteMethodLong, HttpMethod.Delete, 6);
        SetKnownMethod(_mask8Chars, _httpConnectMethodLong, HttpMethod.Connect, 7);
        SetKnownMethod(_mask8Chars, _httpOptionsMethodLong, HttpMethod.Options, 7);
        FillKnownMethodsGaps();
        _methodNames[(byte)HttpMethod.Connect] = HttpMethods.Connect;
        _methodNames[(byte)HttpMethod.Delete] = HttpMethods.Delete;
        _methodNames[(byte)HttpMethod.Get] = HttpMethods.Get;
        _methodNames[(byte)HttpMethod.Head] = HttpMethods.Head;
        _methodNames[(byte)HttpMethod.Options] = HttpMethods.Options;
        _methodNames[(byte)HttpMethod.Patch] = HttpMethods.Patch;
        _methodNames[(byte)HttpMethod.Post] = HttpMethods.Post;
        _methodNames[(byte)HttpMethod.Put] = HttpMethods.Put;
        _methodNames[(byte)HttpMethod.Trace] = HttpMethods.Trace;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetKnownMethod(ulong mask, ulong knownMethodUlong, HttpMethod knownMethod, int length)
    {
        _knownMethods[GetKnownMethodIndex(knownMethodUlong)] = new Tuple<ulong, ulong, HttpMethod, int>(mask, knownMethodUlong, knownMethod, length);
    }

    private static void FillKnownMethodsGaps()
    {
        var knownMethods = _knownMethods;
        var length = knownMethods.Length;
        var invalidHttpMethod = new Tuple<ulong, ulong, HttpMethod, int>(_mask8Chars, 0ul, HttpMethod.Custom, 0);
        for (int i = 0; i < length; i++)
        {
            if (knownMethods[i] == null)
            {
                knownMethods[i] = invalidHttpMethod;
            }
        }
    }

    private static ulong GetMaskAsLong(ReadOnlySpan<byte> bytes)
    {
        Debug.Assert(bytes.Length == 8, "Mask must be exactly 8 bytes long.");

        return BinaryPrimitives.ReadUInt64LittleEndian(bytes);
    }

    private static ulong GetAsciiStringAsLong(string str)
    {
        Debug.Assert(str.Length == 8, "String must be exactly 8 (ASCII) characters long.");

        Span<byte> bytes = stackalloc byte[8];
        OperationStatus operationStatus = Ascii.FromUtf16(str, bytes, out _);

        Debug.Assert(operationStatus == OperationStatus.Done);

        return BinaryPrimitives.ReadUInt64LittleEndian(bytes);
    }

    public static string GetAsciiStringEscaped(this ReadOnlySpan<byte> span, int maxChars)
    {
        var sb = new StringBuilder();

        for (var i = 0; i < Math.Min(span.Length, maxChars); i++)
        {
            var ch = span[i];
            sb.Append(ch < 0x20 || ch >= 0x7F ? $"\\x{ch:X2}" : ((char)ch).ToString());
        }

        if (span.Length > maxChars)
        {
            sb.Append("...");
        }

        return sb.ToString();
    }

    public static string GetAsciiString(this Span<byte> span)
    => StringUtilities.GetAsciiString(span);

    public static string GetAsciiOrUTF8String(this ReadOnlySpan<byte> span)
        => StringUtilities.GetAsciiOrUTF8String(span, DefaultRequestHeaderEncoding);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HttpMethod GetKnownMethod(this ReadOnlySpan<byte> span, out int methodLength)
    {
        methodLength = 0;
        if (sizeof(uint) <= span.Length)
        {
            if (BinaryPrimitives.ReadUInt32LittleEndian(span) == _httpGetMethodInt)
            {
                methodLength = 3;
                return HttpMethod.Get;
            }
            else if (sizeof(ulong) <= span.Length)
            {
                var value = BinaryPrimitives.ReadUInt64LittleEndian(span);
                var index = GetKnownMethodIndex(value);
                var knownMethods = _knownMethods;
                if ((uint)index < (uint)knownMethods.Length)
                {
                    var knownMethod = _knownMethods[index];

                    if (knownMethod != null && (value & knownMethod.Item1) == knownMethod.Item2)
                    {
                        methodLength = knownMethod.Item4;
                        return knownMethod.Item3;
                    }
                }
            }
        }

        return HttpMethod.Custom;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetKnownMethodIndex(ulong value)
    {
        const int magicNumer = 0x600000C;
        var tmp = (int)value & magicNumer;
        return ((tmp >> 2) | (tmp >> 23)) & 0xF;
    }

    /// <summary>
    /// Checks 8 bytes from <paramref name="span"/>  correspond to a known HTTP version.
    /// </summary>
    /// <remarks>
    /// A "known HTTP version" Is is either HTTP/1.0 or HTTP/1.1.
    /// Since those fit in 8 bytes, they can be optimally looked up by reading those bytes as a long. Once
    /// in that format, it can be checked against the known versions.
    /// To optimize performance the HTTP/1.1 will be checked first.
    /// </remarks>
    /// <returns>the HTTP version if the input matches a known string, <c>Unknown</c> otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static HttpVersion GetKnownVersion(this ReadOnlySpan<byte> span)
    {
        if (BinaryPrimitives.TryReadUInt64LittleEndian(span, out var version))
        {
            if (version == _http11VersionLong)
            {
                return HttpVersion.Http11;
            }
            else if (version == _http10VersionLong)
            {
                return HttpVersion.Http10;
            }
        }
        return HttpVersion.Unknown;
    }

    public static string DecodePath(Span<byte> path, bool pathEncoded, string rawTarget, int queryLength)
    {
        int pathLength;
        if (pathEncoded)
        {
            // URI was encoded, unescape and then parse as UTF-8
            pathLength = UrlDecoder.DecodeInPlace(path, isFormEncoding: false);

            // Removing dot segments must be done after unescaping. From RFC 3986:
            //
            // URI producing applications should percent-encode data octets that
            // correspond to characters in the reserved set unless these characters
            // are specifically allowed by the URI scheme to represent data in that
            // component.  If a reserved character is found in a URI component and
            // no delimiting role is known for that character, then it must be
            // interpreted as representing the data octet corresponding to that
            // character's encoding in US-ASCII.
            //
            // https://tools.ietf.org/html/rfc3986#section-2.2
            pathLength = PathNormalizer.RemoveDotSegments(path.Slice(0, pathLength));

            return Encoding.UTF8.GetString(path.Slice(0, pathLength));
        }

        pathLength = PathNormalizer.RemoveDotSegments(path);

        if (path.Length == pathLength && queryLength == 0)
        {
            // If no decoding was required, no dot segments were removed and
            // there is no query, the request path is the same as the raw target
            return rawTarget;
        }

        return path.Slice(0, pathLength).GetAsciiString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetKnownHttpScheme(this Span<byte> span, out HttpScheme knownScheme)
    {
        if (BinaryPrimitives.TryReadUInt64LittleEndian(span, out var scheme))
        {
            if ((scheme & _mask7Chars) == _httpSchemeLong)
            {
                knownScheme = HttpScheme.Http;
                return true;
            }

            if (scheme == _httpsSchemeLong)
            {
                knownScheme = HttpScheme.Https;
                return true;
            }
        }
        knownScheme = HttpScheme.Unknown;
        return false;
    }
}