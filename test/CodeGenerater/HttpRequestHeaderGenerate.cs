using Microsoft.Extensions.Primitives;
using NZ.Orz.Http;
using System.Data;
using System.Text;

namespace CodeGenerater;

public class HttpRequestHeaderGenerate
{
    private Dictionary<string, string> headers = new()
    {
        {nameof(HeaderNames.Host),HeaderNames.Host},
{nameof(HeaderNames.Connection),HeaderNames.Connection},
{nameof(HeaderNames.ContentLength),HeaderNames.ContentLength},
{nameof(HeaderNames.UserAgent),HeaderNames.UserAgent},
{nameof(HeaderNames.Upgrade),HeaderNames.Upgrade},
{nameof(HeaderNames.UpgradeInsecureRequests),HeaderNames.UpgradeInsecureRequests},
{nameof(HeaderNames.Cookie),HeaderNames.Cookie},
{nameof(HeaderNames.TraceParent),HeaderNames.TraceParent},
{nameof(HeaderNames.TraceState),HeaderNames.TraceState},
{nameof(HeaderNames.XForwardedFor),HeaderNames.XForwardedFor},
{nameof(HeaderNames.XForwardedHost),HeaderNames.XForwardedHost},
{nameof(HeaderNames.XForwardedProto),HeaderNames.XForwardedProto},
{nameof(HeaderNames.Origin),HeaderNames.Origin},
{nameof(HeaderNames.CacheControl),HeaderNames.CacheControl},
{nameof(HeaderNames.ContentType),HeaderNames.ContentType},
{nameof(HeaderNames.AccessControlRequestMethod),HeaderNames.AccessControlRequestMethod},
{nameof(HeaderNames.AccessControlRequestHeaders),HeaderNames.AccessControlRequestHeaders},
{nameof(HeaderNames.XRequestID),HeaderNames.XRequestID},
{nameof(HeaderNames.Accept),HeaderNames.Accept},
{nameof(HeaderNames.AcceptCharset),HeaderNames.AcceptCharset},
{nameof(HeaderNames.AcceptDatetime),HeaderNames.AcceptDatetime},
{nameof(HeaderNames.AcceptEncoding),HeaderNames.AcceptEncoding},
{nameof(HeaderNames.AcceptLanguage),HeaderNames.AcceptLanguage},
{nameof(HeaderNames.ContentEncoding),HeaderNames.ContentEncoding},
{nameof(HeaderNames.ContentMD5),HeaderNames.ContentMD5},
{nameof(HeaderNames.Expect),HeaderNames.Expect},
{nameof(HeaderNames.IfMatch),HeaderNames.IfMatch},
{nameof(HeaderNames.IfModifiedSince),HeaderNames.IfModifiedSince},
{nameof(HeaderNames.IfNoneMatch),HeaderNames.IfNoneMatch},
{nameof(HeaderNames.IfRange),HeaderNames.IfRange},
{nameof(HeaderNames.IfUnmodifiedSince),HeaderNames.IfUnmodifiedSince},
{nameof(HeaderNames.MaxForwards),HeaderNames.MaxForwards},
{nameof(HeaderNames.Pragma),HeaderNames.Pragma},
{nameof(HeaderNames.Prefer),HeaderNames.Prefer},
{nameof(HeaderNames.ProxyAuthorization),HeaderNames.ProxyAuthorization},
{nameof(HeaderNames.Range),HeaderNames.Range},
{nameof(HeaderNames.Referer),HeaderNames.Referer},
{nameof(HeaderNames.TE),HeaderNames.TE},
{nameof(HeaderNames.Trailer),HeaderNames.Trailer},
{nameof(HeaderNames.TransferEncoding),HeaderNames.TransferEncoding},
{nameof(HeaderNames.ProxyConnection),HeaderNames.ProxyConnection},
{nameof(HeaderNames.XCorrelationID),HeaderNames.XCorrelationID},
{nameof(HeaderNames.CorrelationID),HeaderNames.CorrelationID},
{nameof(HeaderNames.RequestId),HeaderNames.RequestId},
{nameof(HeaderNames.KeepAlive),HeaderNames.KeepAlive},
{nameof(HeaderNames.ProxyAuthenticate),HeaderNames.ProxyAuthenticate},
{nameof(HeaderNames.Forwarded),HeaderNames.Forwarded},
{nameof(HeaderNames.XCsrfToken),HeaderNames.XCsrfToken},
    };

    private Dictionary<string, string> bits = new(StringComparer.OrdinalIgnoreCase)
    {
    };

    public string Generate()
    {
        if (headers.Distinct().Count() != headers.Count)
        {
            throw new DuplicateNameException();
        }
        InitData();
        return $@"using Microsoft.Extensions.Primitives;
using NZ.Orz.Http.Exceptions;
using System.Collections.Frozen;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NZ.Orz.Http;

public partial class HttpRequestHeaders
{{
    {GenerateGetter()}

    private void FastClear()
    {{
        var tempBits = _bits;
        _bits = 0;
        if (BitOperations.PopCount(tempBits) > 12)
        {{
            _r = default(HeaderReferences);
            return;
        }}
        {GenerateClear()}
    }}

    private bool FastAdd(KnownHeaderType k, StringValues value)
    {{
        switch (k)
        {{
            {GenerateAdd()}

            case KnownHeaderType.Unknown:
            default:
                return false;
        }}
    }}

    private bool FastRemove(KnownHeaderType k)
    {{
        switch (k)
        {{
            {GenerateRemove()}

            case KnownHeaderType.Unknown:
            default:
                return false;
        }}
    }}

    private bool FastTryGetValue(KnownHeaderType k, out StringValues value)
    {{
        switch (k)
        {{
            {GenerateTryGetValue()}

            case KnownHeaderType.Unknown:
            default:
                break;
        }}
        value = default;
        return false;
    }}

    public void CopyTo(KeyValuePair<string, StringValues>[] array, int arrayIndex)
    {{
        if (arrayIndex < 0)
        {{
            return;
        }}
        {GenerateCopyTo()}
        ((ICollection<KeyValuePair<string, StringValues>>?)dict)?.CopyTo(array, arrayIndex);
    }}

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void Append(ReadOnlySpan<byte> name, ReadOnlySpan<byte> value, bool checkForNewlineChars)
    {{
        var nameStr = string.Empty;
        ref byte nameStart = ref MemoryMarshal.GetReference(name);
        ref StringValues values = ref Unsafe.NullRef<StringValues>();
        var flag = 0UL;
        switch (name.Length)
        {{
            {GenerateAppend()}

            default:
                break;
        }}
        if (flag != 0UL)
        {{
            var valueStr = value.GetRequestHeaderString(nameStr, checkForNewlineChars);
            if ((_bits & flag) == 0)
            {{
                _bits |= flag;
                values = new StringValues(valueStr);
            }}
            else
            {{
                values = StringValues.Concat(values, valueStr);
            }}
        }}
        else
        {{
            nameStr = name.GetHeaderName();
            var valueStr = value.GetRequestHeaderString(nameStr, checkForNewlineChars);
            dict.TryGetValue(nameStr, out var existing);
            dict[nameStr] = StringValues.Concat(existing, valueStr);
        }}
    }}

    public partial struct Enumerator
    {{
        public bool MoveNext()
        {{
            switch (_next)
            {{
                {GenerateMoveNext()}

                default:
                    if (_enumerator != null && _enumerator.MoveNext())
                    {{
                        _current = _enumerator.Current;
                        return true;
                    }}
                    return false;
            }}

            if (_currentBits != 0)
            {{
                _next = BitOperations.TrailingZeroCount(_currentBits);
                return true;
            }}
            else
            {{
                _next = -1;
                return true;
            }}
        }}
    }}

    private struct HeaderReferences
    {{
        {GenerateFields()}
    }}

    private enum KnownHeaderType
    {{
        Unknown = 0,
        {GenerateKnownHeaderType()}
    }}

    private static readonly FrozenDictionary<string, KnownHeaderType> _internedHeaderType = new Dictionary<string, KnownHeaderType>(StringComparer.OrdinalIgnoreCase)
    {{
        {GenerateInternedHeaderType()}
    }}.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
}}
";
    }

    private string GenerateAppend()
    {
        var sb = new StringBuilder();

        foreach (var kvs in bits.Select(i => (i.Key, Encoding.ASCII.GetBytes(headers[i.Key]), i.Value)).GroupBy(i => i.Item2.Length).OrderBy(i => i.Key))
        {
            var c = kvs.Key;
            var a = kvs.ToArray();
            sb.AppendLine($"case {c}:{{");
            List<(string, string[])> np = new();
            var pi = 0;
            var idex = 0;
            sb.AppendLine($"ref byte ns = ref nameStart;");
            while (c > 0)
            {
                if (c >= 8)
                {
                    sb.AppendLine($"var l{pi}= ReadUnalignedLittleEndian_ulong(ref ns);");
                    c -= 8;
                    if (c > 0)
                    {
                        sb.AppendLine($"ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(8));");
                    }
                    np.Add(($"l{pi}", a.Select(i => $"{HttpRequestHeaders.ReadUnalignedLittleEndian_ulong(ref i.Item2[idex])}UL").ToArray()));
                    pi++;
                    idex += 8;
                }
                else if (c >= 4)
                {
                    sb.AppendLine($"var i{pi}= ReadUnalignedLittleEndian_uint(ref ns);");
                    c -= 4;
                    if (c > 0)
                    {
                        sb.AppendLine($"ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(4));");
                    }
                    np.Add(($"i{pi}", a.Select(i => $"{HttpRequestHeaders.ReadUnalignedLittleEndian_uint(ref i.Item2[idex])}U").ToArray()));
                    pi++;
                    idex += 4;
                }
                else if (c >= 2)
                {
                    sb.AppendLine($"var s{pi}= ReadUnalignedLittleEndian_ushort(ref ns);");
                    c -= 2;
                    if (c > 0)
                    {
                        sb.AppendLine($"ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(2));");
                    }
                    np.Add(($"s{pi}", a.Select(i => $"{HttpRequestHeaders.ReadUnalignedLittleEndian_ushort(ref i.Item2[idex])}").ToArray()));
                    pi++;
                    idex += 2;
                }
                else
                {
                    np.Add(("ns", a.Select(i => $"{i.Item2[idex]}").ToArray()));
                    c--;
                    idex++;
                }
            }

            for (int i = 0; i < a.Length; i++)
            {
                var (k, kbs, v) = a[i];
                sb.Append(i == 0 ? "if" : " else if");
                sb.Append(" (");
                sb.Append(string.Join(" && ", np.Select(x =>
                {
                    return $"{x.Item1} == {x.Item2[i]}";
                })));
                sb.AppendLine(")");
                if (k == nameof(HeaderNames.ContentLength))
                {
                    sb.AppendLine(@$"{{
                    if ((_bits & {v}) == 0)
                    {{
                        _bits |= {v};
                        AppendContentLength(value);
                        return;
                    }}
                    else
                    {{
                        throw BadHttpRequestException.GetException(RequestRejectionReason.MultipleContentLengths);
                    }}
}}");
                }
                else
                {
                    sb.AppendLine(@$"{{
                    flag = {v};
                    values = ref _r.{k};
                    nameStr = HeaderNames.{k};
}}");
                }
            }
            sb.AppendLine($"}} break;");
        }
        var r = sb.ToString();
        return r;
    }

    private string GenerateMoveNext()
    {
        var sb = new StringBuilder();
        var index = 0;
        foreach (var (k, v) in bits)
        {
            if (k == nameof(HeaderNames.ContentLength))
            {
                sb.AppendLine(@$"
 case {index}:
    _current = new KeyValuePair<string, StringValues>(HeaderNames.ContentLength, HeaderUtilities.FormatNonNegativeInt64(_collection._contentLength.Value));
    _currentBits &= ~{v};
    break;
");
            }
            else
            {
                sb.AppendLine(@$"
 case {index}:
    _current = new KeyValuePair<string, StringValues>(HeaderNames.{k}, _collection._r.{k});
    _currentBits &= ~{v};
    break;
");
            }
            index++;
        }
        var r = sb.ToString();
        return r;
    }

    private string GenerateCopyTo()
    {
        var sb = new StringBuilder();

        foreach (var (k, v) in bits)
        {
            if (k == nameof(HeaderNames.ContentLength))
            {
                sb.AppendLine(@$"
if ((_bits & {v}) != 0UL)
{{
    if (arrayIndex == array.Length)
    {{
        return;
    }}
    array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.{k}, HeaderUtilities.FormatNonNegativeInt64(_contentLength.Value));
    ++arrayIndex;
}}
");
            }
            else
            {
                sb.AppendLine(@$"
if ((_bits & {v}) != 0UL)
{{
    if (arrayIndex == array.Length)
    {{
        return;
    }}
    array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.{k}, _r.{k});
    ++arrayIndex;
}}
");
            }
        }
        var r = sb.ToString();
        return r;
    }

    private string GenerateTryGetValue()
    {
        var sb = new StringBuilder();

        foreach (var (k, v) in bits)
        {
            if (k == nameof(HeaderNames.ContentLength))
            {
                sb.AppendLine(@$"
case KnownHeaderType.ContentLength:
    if ((_bits & {v}) != 0UL)
    {{
        value = HeaderUtilities.FormatNonNegativeInt64(_contentLength.Value);
        return true;
    }}
    break;
");
            }
            else
            {
                sb.AppendLine(@$"
case KnownHeaderType.{k}:
    if ((_bits & {v}) != 0UL)
    {{
        value = _r.{k};
        return true;
    }}
    break;
");
            }
        }
        var r = sb.ToString();
        return r;
    }

    private string GenerateRemove()
    {
        var sb = new StringBuilder();

        foreach (var (k, v) in bits)
        {
            if (k == nameof(HeaderNames.ContentLength))
            {
                sb.AppendLine(@$"
case KnownHeaderType.ContentLength:
    if ((_bits & {v}) != 0UL)
    {{
        _bits &= ~{v};
        _contentLength = default;
        return true;
    }}
    return true;
");
            }
            else
            {
                sb.AppendLine(@$"
case KnownHeaderType.{k}:
    if ((_bits & {v}) != 0UL)
    {{
        _bits &= ~{v};
        _r.{k} = default;
        return true;
    }}
    return false;
");
            }
        }
        var r = sb.ToString();
        return r;
    }

    private void InitData()
    {
        ulong b = 1UL;
        foreach (var item in headers.Keys)
        {
            bits.Add(item, $"{b}UL");
            b <<= 1;
            if (b == 0) break;
        }
    }

    private string GenerateAdd()
    {
        var sb = new StringBuilder();

        foreach (var (k, v) in bits)
        {
            if (k == nameof(HeaderNames.ContentLength))
            {
                sb.AppendLine(@$"
case KnownHeaderType.ContentLength:
    _bits |= {v};
    _contentLength = ParseContentLength(value.ToString());
    return true;
");
            }
            else
            {
                sb.AppendLine(@$"
case KnownHeaderType.{k}:
    {k} = value;
    return true;
");
            }
        }
        var r = sb.ToString();
        return r;
    }

    private string GenerateInternedHeaderType()
    {
        var sb = new StringBuilder();

        foreach (var (k, v) in bits)
        {
            sb.AppendLine(@$"{{HeaderNames.{k},KnownHeaderType.{k}}},");
        }
        var r = sb.ToString();
        return r;
    }

    private string GenerateKnownHeaderType()
    {
        var sb = new StringBuilder();

        foreach (var (k, v) in bits)
        {
            sb.AppendLine(@$"{k},");
        }
        var r = sb.ToString();
        return r;
    }

    private string GenerateGetter()
    {
        var sb = new StringBuilder();

        foreach (var (k, v) in bits)
        {
            if (k == nameof(HeaderNames.ContentLength))
            {
                sb.AppendLine(@$"
public long? ContentLength
{{
    get {{ return _contentLength; }}
    set
    {{
        if (value.HasValue)
        {{
            _bits |= {v};
            _contentLength = value;
        }}
        else
        {{
            _bits &= ~{v};
            _contentLength = default;
        }}
    }}
}}
");
            }
            else
            {
                sb.AppendLine(@$"
public StringValues {k}
{{
    get {{ return _r.{k}; }}
    set
    {{
        if (!StringValues.IsNullOrEmpty(value))
        {{
            _bits |= {v};
            _r.{k} = value;
        }}
        else
        {{
            _bits &= ~{v};
            _r.{k} = default;
        }}
    }}
}}
");
            }

            if (k == nameof(HeaderNames.Connection))
            {
                sb.AppendLine(@$"public bool HasConnection => (_bits & {v}) != 0;");
            }
            else if (k == nameof(HeaderNames.Cookie))
            {
                sb.AppendLine(@$"public bool HasCookie => (_bits & {v}) != 0;");
            }
            else if (k == nameof(HeaderNames.TransferEncoding))
            {
                sb.AppendLine(@$"public bool HasTransferEncoding => (_bits & {v}) != 0;");
            }
        }
        var r = sb.ToString();
        return r;
    }

    private string GenerateClear()
    {
        var sb = new StringBuilder();

        foreach (var (k, v) in bits)
        {
            if (k == nameof(HeaderNames.ContentLength))
            {
                sb.AppendLine(@$"
if ((_bits & {v}) != 0UL)
{{
    _contentLength = null;
}}
");
            }
            else
            {
                sb.AppendLine(@$"

if ((_bits & {v}) != 0UL)
{{
    _r.{k} = default;
}}
");
            }
        }
        var r = sb.ToString();
        return r;
    }

    private string GenerateFields()
    {
        var sb = new StringBuilder();

        foreach (var (k, v) in bits)
        {
            if (k == nameof(HeaderNames.ContentLength))
            {
            }
            else
            {
                sb.AppendLine(@$"public StringValues {k};");
            }
        }
        var r = sb.ToString();
        return r;
    }
}