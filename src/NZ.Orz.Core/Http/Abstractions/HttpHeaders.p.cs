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
        r._ContentLength = StringValues.Empty;
    }

    private bool FastAdd(string k, StringValues value)
    {
        var r = _references;
        switch (k.Length)
        {
            case 13:
                if (ReferenceEquals(HeaderNames.ContentLength, k))
                {
                    r._ContentLength = value;
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
                    if (!ReferenceEquals(StringValues.Empty, r._ContentLength))
                    {
                        r._ContentLength = StringValues.Empty;
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
                if (ReferenceEquals(HeaderNames.ContentLength, k))
                {
                    if (ReferenceEquals(StringValues.Empty, r._ContentLength))
                    {
                        return false;
                    }
                    else
                    {
                        value = r._ContentLength;
                        return true;
                    }
                }
                return false;

            default:
                return false;
        }
    }

    private struct HeaderReferences
    {
        public StringValues _ContentLength;
        public StringValues _Accept;
        public StringValues _Connection;
        public StringValues _Host;
        public StringValues _UserAgent;
        public StringValues _Authority;
        public StringValues _Method;
        public StringValues _Path;
        public StringValues _Protocol;
        public StringValues _Scheme;
        public StringValues _AcceptCharset;
        public StringValues _AcceptEncoding;
        public StringValues _AcceptLanguage;
        public StringValues _AccessControlRequestHeaders;
        public StringValues _AccessControlRequestMethod;
        public StringValues _AltUsed;
        public StringValues _Authorization;
        public StringValues _Baggage;
        public StringValues _CacheControl;
        public StringValues _ContentType;
        public StringValues _Cookie;
        public StringValues _CorrelationContext;
        public StringValues _Date;
        public StringValues _Expect;
        public StringValues _From;
        public StringValues _GrpcAcceptEncoding;
        public StringValues _GrpcEncoding;
        public StringValues _GrpcTimeout;
        public StringValues _IfMatch;
        public StringValues _IfModifiedSince;
        public StringValues _IfNoneMatch;
        public StringValues _IfRange;
        public StringValues _IfUnmodifiedSince;
        public StringValues _KeepAlive;
        public StringValues _MaxForwards;
        public StringValues _Origin;
        public StringValues _Pragma;
        public StringValues _ProxyAuthorization;
        public StringValues _Range;
        public StringValues _Referer;
        public StringValues _RequestId;
        public StringValues _TE;
        public StringValues _TraceParent;
        public StringValues _TraceState;
        public StringValues _TransferEncoding;
        public StringValues _Translate;
        public StringValues _Upgrade;
        public StringValues _UpgradeInsecureRequests;
        public StringValues _Via;
        public StringValues _Warning;
    }
}