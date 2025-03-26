using Microsoft.Extensions.Primitives;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace NZ.Orz.Http;

public partial class HttpHeaders
{
    private HeaderReferences _references = new HeaderReferences();
    private long? _contentLength;

    public long? ContentLength
    {
        get { return _contentLength; }
        set
        {
            _contentLength = value;
        }
    }

    private void FastClear()
    {
        var r = _references;
        r.CacheControl = StringValues.Empty;
    }

    private bool FastAdd(string k, StringValues value)
    {
        var r = _references;
        switch (k.Length)
        {
            case 13:
                if (ReferenceEquals(HeaderNames.CacheControl, k))
                {
                    r.CacheControl = value;
                    return true;
                }

                return false;

            default:
                return false;
        }
    }

    private bool FastRemove(string k)
    {
        var r = _references;
        switch (k.Length)
        {
            case 13:
                if (ReferenceEquals(HeaderNames.ContentLength, k))
                {
                    if (StringValues.Empty != r.CacheControl)
                    {
                        r.CacheControl = StringValues.Empty;
                        return true;
                    }
                    return false;
                }

                return false;

            default:
                return false;
        }
    }

    private bool FastTryGetValue(string k, out StringValues value)
    {
        var r = _references;
        switch (k.Length)
        {
            case 13:
                if (ReferenceEquals(HeaderNames.CacheControl, k))
                {
                    if (StringValues.Empty != r.CacheControl)
                    {
                        value = r.CacheControl;
                        return true;
                    }
                }
                break;

            default:
                break;
        }
        value = StringValues.Empty;
        return false;
    }

    private struct HeaderReferences
    {
        public StringValues Accept;
        public StringValues Connection;
        public StringValues Host;
        public StringValues UserAgent;
        public StringValues Authority;
        public StringValues Method;
        public StringValues Path;
        public StringValues Protocol;
        public StringValues Scheme;
        public StringValues AcceptCharset;
        public StringValues AcceptEncoding;
        public StringValues AcceptLanguage;
        public StringValues AccessControlRequestHeaders;
        public StringValues AccessControlRequestMethod;
        public StringValues AltUsed;
        public StringValues Authorization;
        public StringValues Baggage;
        public StringValues CacheControl;
        public StringValues ContentType;
        public StringValues Cookie;
        public StringValues CorrelationContext;
        public StringValues Date;
        public StringValues Expect;
        public StringValues From;
        public StringValues GrpcAcceptEncoding;
        public StringValues GrpcEncoding;
        public StringValues GrpcTimeout;
        public StringValues IfMatch;
        public StringValues IfModifiedSince;
        public StringValues IfNoneMatch;
        public StringValues IfRange;
        public StringValues IfUnmodifiedSince;
        public StringValues KeepAlive;
        public StringValues MaxForwards;
        public StringValues Origin;
        public StringValues Pragma;
        public StringValues ProxyAuthorization;
        public StringValues Range;
        public StringValues Referer;
        public StringValues RequestId;
        public StringValues TE;
        public StringValues TraceParent;
        public StringValues TraceState;
        public StringValues TransferEncoding;
        public StringValues Translate;
        public StringValues Upgrade;
        public StringValues UpgradeInsecureRequests;
        public StringValues Via;
        public StringValues Warning;
    }
}