using System.Data;

namespace CodeGenerater;

public class HttpRequestHeaderGenerate
{
    private List<string> headers = new List<string>()
    {
        "Host",
"ContentLength",
"ContentType",
"Connection",
"UserAgent",
"Accept",
"AcceptCharset",
"AcceptEncoding",
"AcceptLanguage",
"AcceptRanges",
"AccessControlAllowCredentials",
"AccessControlAllowHeaders",
"AccessControlAllowMethods",
"AccessControlAllowOrigin",
"AccessControlExposeHeaders",
"AccessControlMaxAge",
"AccessControlRequestHeaders",
"AccessControlRequestMethod",
"Age",
"Allow",
"AltSvc",
"AltUsed",
"Authority",
"Authorization",
"Baggage",
"CacheControl",
"ContentDisposition",
"ContentEncoding",
"ContentLanguage",
"ContentLocation",
"ContentMD5",
"ContentRange",
"ContentSecurityPolicy",
"ContentSecurityPolicyReportOnly",
"Cookie",
"CorrelationContext",
"Date",
"DNT",
"ETag",
"Expect",
"Expires",
"From",
"GrpcAcceptEncoding",
"GrpcEncoding",
"GrpcMessage",
"GrpcStatus",
"GrpcTimeout",
"IfMatch",
"IfModifiedSince",
"IfNoneMatch",
"IfRange",
"IfUnmodifiedSince",
"KeepAlive",
"LastModified",
"Link",
"Location",
"MaxForwards",
"Method",
"Origin",
"Path",
"Pragma",
"Protocol",
"ProxyAuthenticate",
"ProxyAuthorization",
"ProxyConnection",
"Range",
"Referer",
"RequestId",
"SecWebSocketAccept",
"SecWebSocketKey",
"SecWebSocketProtocol",
"SecWebSocketVersion",
"SecWebSocketExtensions",
"StrictTransportSecurity",
"RetryAfter",
"Scheme",
"Server",
"SetCookie",
"TE",
"TraceParent",
"TraceState",
"Trailer",
"TransferEncoding",
"Translate",
"Upgrade",
"UpgradeInsecureRequests",
"Vary",
"Via",
"Warning",
"WWWAuthenticate",
"XContentTypeOptions",
"XFrameOptions",
"XPoweredBy",
"XRequestedWith",
"XUACompatible",
"XXSSProtection",
    };

    private Dictionary<string, ulong> bits = new(StringComparer.OrdinalIgnoreCase)
    {
    };

    public string Generate()
    {
        if (headers.Distinct().Count() != headers.Count)
        {
            throw new DuplicateNameException();
        }
        InitData();
        return $@"
using Microsoft.Extensions.Primitives;
using System.Numerics;

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

    private string GenerateMoveNext()
    {
        throw new NotImplementedException();
    }

    private string GenerateCopyTo()
    {
        throw new NotImplementedException();
    }

    private string GenerateTryGetValue()
    {
        throw new NotImplementedException();
    }

    private string GenerateRemove()
    {
        throw new NotImplementedException();
    }

    private void InitData()
    {
        ulong b = 1UL;
        foreach (var item in headers)
        {
            bits.Add(item, b);
            b <<= 1;
            if (b == 0) break;
        }
    }

    private string GenerateAdd()
    {
        throw new NotImplementedException();
    }

    private string GenerateInternedHeaderType()
    {
        throw new NotImplementedException();
    }

    private string GenerateKnownHeaderType()
    {
        throw new NotImplementedException();
    }

    private string GenerateGetter()
    {
        throw new NotImplementedException();
    }

    private string GenerateClear()
    {
        throw new NotImplementedException();
    }

    private string GenerateFields()
    {
        throw new NotImplementedException();
    }
}