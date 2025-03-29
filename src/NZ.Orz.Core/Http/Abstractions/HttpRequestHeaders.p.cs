using Microsoft.Extensions.Primitives;
using System.Collections.Frozen;
using System.Numerics;

namespace NZ.Orz.Http;

public partial class HttpRequestHeaders
{
    public StringValues Host
    {
        get { return _r.Host; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 1UL;
                _r.Host = value;
            }
            else
            {
                _bits &= ~1UL;
                _r.Host = default;
            }
        }
    }

    public StringValues Connection
    {
        get { return _r.Connection; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 2UL;
                _r.Connection = value;
            }
            else
            {
                _bits &= ~2UL;
                _r.Connection = default;
            }
        }
    }

    public long? ContentLength
    {
        get { return _contentLength; }
        set
        {
            if (value.HasValue)
            {
                _bits |= 4UL;
                _contentLength = value;
            }
            else
            {
                _bits &= ~4UL;
                _contentLength = default;
            }
        }
    }

    public StringValues UserAgent
    {
        get { return _r.UserAgent; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 8UL;
                _r.UserAgent = value;
            }
            else
            {
                _bits &= ~8UL;
                _r.UserAgent = default;
            }
        }
    }

    public StringValues Upgrade
    {
        get { return _r.Upgrade; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 16UL;
                _r.Upgrade = value;
            }
            else
            {
                _bits &= ~16UL;
                _r.Upgrade = default;
            }
        }
    }

    public StringValues UpgradeInsecureRequests
    {
        get { return _r.UpgradeInsecureRequests; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 32UL;
                _r.UpgradeInsecureRequests = value;
            }
            else
            {
                _bits &= ~32UL;
                _r.UpgradeInsecureRequests = default;
            }
        }
    }

    public StringValues Cookie
    {
        get { return _r.Cookie; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 64UL;
                _r.Cookie = value;
            }
            else
            {
                _bits &= ~64UL;
                _r.Cookie = default;
            }
        }
    }

    public StringValues TraceParent
    {
        get { return _r.TraceParent; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 128UL;
                _r.TraceParent = value;
            }
            else
            {
                _bits &= ~128UL;
                _r.TraceParent = default;
            }
        }
    }

    public StringValues TraceState
    {
        get { return _r.TraceState; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 256UL;
                _r.TraceState = value;
            }
            else
            {
                _bits &= ~256UL;
                _r.TraceState = default;
            }
        }
    }

    public StringValues XForwardedFor
    {
        get { return _r.XForwardedFor; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 512UL;
                _r.XForwardedFor = value;
            }
            else
            {
                _bits &= ~512UL;
                _r.XForwardedFor = default;
            }
        }
    }

    public StringValues XForwardedHost
    {
        get { return _r.XForwardedHost; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 1024UL;
                _r.XForwardedHost = value;
            }
            else
            {
                _bits &= ~1024UL;
                _r.XForwardedHost = default;
            }
        }
    }

    public StringValues XForwardedProto
    {
        get { return _r.XForwardedProto; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 2048UL;
                _r.XForwardedProto = value;
            }
            else
            {
                _bits &= ~2048UL;
                _r.XForwardedProto = default;
            }
        }
    }

    public StringValues Origin
    {
        get { return _r.Origin; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 4096UL;
                _r.Origin = value;
            }
            else
            {
                _bits &= ~4096UL;
                _r.Origin = default;
            }
        }
    }

    public StringValues CacheControl
    {
        get { return _r.CacheControl; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 8192UL;
                _r.CacheControl = value;
            }
            else
            {
                _bits &= ~8192UL;
                _r.CacheControl = default;
            }
        }
    }

    public StringValues ContentType
    {
        get { return _r.ContentType; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 16384UL;
                _r.ContentType = value;
            }
            else
            {
                _bits &= ~16384UL;
                _r.ContentType = default;
            }
        }
    }

    public StringValues AccessControlRequestMethod
    {
        get { return _r.AccessControlRequestMethod; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 32768UL;
                _r.AccessControlRequestMethod = value;
            }
            else
            {
                _bits &= ~32768UL;
                _r.AccessControlRequestMethod = default;
            }
        }
    }

    public StringValues AccessControlRequestHeaders
    {
        get { return _r.AccessControlRequestHeaders; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 65536UL;
                _r.AccessControlRequestHeaders = value;
            }
            else
            {
                _bits &= ~65536UL;
                _r.AccessControlRequestHeaders = default;
            }
        }
    }

    public StringValues XRequestID
    {
        get { return _r.XRequestID; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 131072UL;
                _r.XRequestID = value;
            }
            else
            {
                _bits &= ~131072UL;
                _r.XRequestID = default;
            }
        }
    }

    public StringValues Accept
    {
        get { return _r.Accept; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 262144UL;
                _r.Accept = value;
            }
            else
            {
                _bits &= ~262144UL;
                _r.Accept = default;
            }
        }
    }

    public StringValues AcceptCharset
    {
        get { return _r.AcceptCharset; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 524288UL;
                _r.AcceptCharset = value;
            }
            else
            {
                _bits &= ~524288UL;
                _r.AcceptCharset = default;
            }
        }
    }

    public StringValues AcceptDatetime
    {
        get { return _r.AcceptDatetime; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 1048576UL;
                _r.AcceptDatetime = value;
            }
            else
            {
                _bits &= ~1048576UL;
                _r.AcceptDatetime = default;
            }
        }
    }

    public StringValues AcceptEncoding
    {
        get { return _r.AcceptEncoding; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 2097152UL;
                _r.AcceptEncoding = value;
            }
            else
            {
                _bits &= ~2097152UL;
                _r.AcceptEncoding = default;
            }
        }
    }

    public StringValues AcceptLanguage
    {
        get { return _r.AcceptLanguage; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 4194304UL;
                _r.AcceptLanguage = value;
            }
            else
            {
                _bits &= ~4194304UL;
                _r.AcceptLanguage = default;
            }
        }
    }

    public StringValues ContentEncoding
    {
        get { return _r.ContentEncoding; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 8388608UL;
                _r.ContentEncoding = value;
            }
            else
            {
                _bits &= ~8388608UL;
                _r.ContentEncoding = default;
            }
        }
    }

    public StringValues ContentMD5
    {
        get { return _r.ContentMD5; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 16777216UL;
                _r.ContentMD5 = value;
            }
            else
            {
                _bits &= ~16777216UL;
                _r.ContentMD5 = default;
            }
        }
    }

    public StringValues Expect
    {
        get { return _r.Expect; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 33554432UL;
                _r.Expect = value;
            }
            else
            {
                _bits &= ~33554432UL;
                _r.Expect = default;
            }
        }
    }

    public StringValues IfMatch
    {
        get { return _r.IfMatch; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 67108864UL;
                _r.IfMatch = value;
            }
            else
            {
                _bits &= ~67108864UL;
                _r.IfMatch = default;
            }
        }
    }

    public StringValues IfModifiedSince
    {
        get { return _r.IfModifiedSince; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 134217728UL;
                _r.IfModifiedSince = value;
            }
            else
            {
                _bits &= ~134217728UL;
                _r.IfModifiedSince = default;
            }
        }
    }

    public StringValues IfNoneMatch
    {
        get { return _r.IfNoneMatch; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 268435456UL;
                _r.IfNoneMatch = value;
            }
            else
            {
                _bits &= ~268435456UL;
                _r.IfNoneMatch = default;
            }
        }
    }

    public StringValues IfRange
    {
        get { return _r.IfRange; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 536870912UL;
                _r.IfRange = value;
            }
            else
            {
                _bits &= ~536870912UL;
                _r.IfRange = default;
            }
        }
    }

    public StringValues IfUnmodifiedSince
    {
        get { return _r.IfUnmodifiedSince; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 1073741824UL;
                _r.IfUnmodifiedSince = value;
            }
            else
            {
                _bits &= ~1073741824UL;
                _r.IfUnmodifiedSince = default;
            }
        }
    }

    public StringValues MaxForwards
    {
        get { return _r.MaxForwards; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 2147483648UL;
                _r.MaxForwards = value;
            }
            else
            {
                _bits &= ~2147483648UL;
                _r.MaxForwards = default;
            }
        }
    }

    public StringValues Pragma
    {
        get { return _r.Pragma; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 4294967296UL;
                _r.Pragma = value;
            }
            else
            {
                _bits &= ~4294967296UL;
                _r.Pragma = default;
            }
        }
    }

    public StringValues Prefer
    {
        get { return _r.Prefer; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 8589934592UL;
                _r.Prefer = value;
            }
            else
            {
                _bits &= ~8589934592UL;
                _r.Prefer = default;
            }
        }
    }

    public StringValues ProxyAuthorization
    {
        get { return _r.ProxyAuthorization; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 17179869184UL;
                _r.ProxyAuthorization = value;
            }
            else
            {
                _bits &= ~17179869184UL;
                _r.ProxyAuthorization = default;
            }
        }
    }

    public StringValues Range
    {
        get { return _r.Range; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 34359738368UL;
                _r.Range = value;
            }
            else
            {
                _bits &= ~34359738368UL;
                _r.Range = default;
            }
        }
    }

    public StringValues Referer
    {
        get { return _r.Referer; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 68719476736UL;
                _r.Referer = value;
            }
            else
            {
                _bits &= ~68719476736UL;
                _r.Referer = default;
            }
        }
    }

    public StringValues TE
    {
        get { return _r.TE; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 137438953472UL;
                _r.TE = value;
            }
            else
            {
                _bits &= ~137438953472UL;
                _r.TE = default;
            }
        }
    }

    public StringValues Trailer
    {
        get { return _r.Trailer; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 274877906944UL;
                _r.Trailer = value;
            }
            else
            {
                _bits &= ~274877906944UL;
                _r.Trailer = default;
            }
        }
    }

    public StringValues TransferEncoding
    {
        get { return _r.TransferEncoding; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 549755813888UL;
                _r.TransferEncoding = value;
            }
            else
            {
                _bits &= ~549755813888UL;
                _r.TransferEncoding = default;
            }
        }
    }

    public StringValues ProxyConnection
    {
        get { return _r.ProxyConnection; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 1099511627776UL;
                _r.ProxyConnection = value;
            }
            else
            {
                _bits &= ~1099511627776UL;
                _r.ProxyConnection = default;
            }
        }
    }

    public StringValues XCorrelationID
    {
        get { return _r.XCorrelationID; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 2199023255552UL;
                _r.XCorrelationID = value;
            }
            else
            {
                _bits &= ~2199023255552UL;
                _r.XCorrelationID = default;
            }
        }
    }

    public StringValues CorrelationID
    {
        get { return _r.CorrelationID; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 4398046511104UL;
                _r.CorrelationID = value;
            }
            else
            {
                _bits &= ~4398046511104UL;
                _r.CorrelationID = default;
            }
        }
    }

    public StringValues RequestId
    {
        get { return _r.RequestId; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 8796093022208UL;
                _r.RequestId = value;
            }
            else
            {
                _bits &= ~8796093022208UL;
                _r.RequestId = default;
            }
        }
    }

    public StringValues KeepAlive
    {
        get { return _r.KeepAlive; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 17592186044416UL;
                _r.KeepAlive = value;
            }
            else
            {
                _bits &= ~17592186044416UL;
                _r.KeepAlive = default;
            }
        }
    }

    public StringValues ProxyAuthenticate
    {
        get { return _r.ProxyAuthenticate; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 35184372088832UL;
                _r.ProxyAuthenticate = value;
            }
            else
            {
                _bits &= ~35184372088832UL;
                _r.ProxyAuthenticate = default;
            }
        }
    }

    public StringValues Forwarded
    {
        get { return _r.Forwarded; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 70368744177664UL;
                _r.Forwarded = value;
            }
            else
            {
                _bits &= ~70368744177664UL;
                _r.Forwarded = default;
            }
        }
    }

    public StringValues XCsrfToken
    {
        get { return _r.XCsrfToken; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 140737488355328UL;
                _r.XCsrfToken = value;
            }
            else
            {
                _bits &= ~140737488355328UL;
                _r.XCsrfToken = default;
            }
        }
    }

    private void FastClear()
    {
        var tempBits = _bits;
        _bits = 0;
        if (BitOperations.PopCount(tempBits) > 12)
        {
            _r = default(HeaderReferences);
            return;
        }

        if ((_bits & 1UL) != 0UL)
        {
            _r.Host = default;
        }

        if ((_bits & 2UL) != 0UL)
        {
            _r.Connection = default;
        }

        if ((_bits & 4UL) != 0UL)
        {
            _contentLength = null;
        }

        if ((_bits & 8UL) != 0UL)
        {
            _r.UserAgent = default;
        }

        if ((_bits & 16UL) != 0UL)
        {
            _r.Upgrade = default;
        }

        if ((_bits & 32UL) != 0UL)
        {
            _r.UpgradeInsecureRequests = default;
        }

        if ((_bits & 64UL) != 0UL)
        {
            _r.Cookie = default;
        }

        if ((_bits & 128UL) != 0UL)
        {
            _r.TraceParent = default;
        }

        if ((_bits & 256UL) != 0UL)
        {
            _r.TraceState = default;
        }

        if ((_bits & 512UL) != 0UL)
        {
            _r.XForwardedFor = default;
        }

        if ((_bits & 1024UL) != 0UL)
        {
            _r.XForwardedHost = default;
        }

        if ((_bits & 2048UL) != 0UL)
        {
            _r.XForwardedProto = default;
        }

        if ((_bits & 4096UL) != 0UL)
        {
            _r.Origin = default;
        }

        if ((_bits & 8192UL) != 0UL)
        {
            _r.CacheControl = default;
        }

        if ((_bits & 16384UL) != 0UL)
        {
            _r.ContentType = default;
        }

        if ((_bits & 32768UL) != 0UL)
        {
            _r.AccessControlRequestMethod = default;
        }

        if ((_bits & 65536UL) != 0UL)
        {
            _r.AccessControlRequestHeaders = default;
        }

        if ((_bits & 131072UL) != 0UL)
        {
            _r.XRequestID = default;
        }

        if ((_bits & 262144UL) != 0UL)
        {
            _r.Accept = default;
        }

        if ((_bits & 524288UL) != 0UL)
        {
            _r.AcceptCharset = default;
        }

        if ((_bits & 1048576UL) != 0UL)
        {
            _r.AcceptDatetime = default;
        }

        if ((_bits & 2097152UL) != 0UL)
        {
            _r.AcceptEncoding = default;
        }

        if ((_bits & 4194304UL) != 0UL)
        {
            _r.AcceptLanguage = default;
        }

        if ((_bits & 8388608UL) != 0UL)
        {
            _r.ContentEncoding = default;
        }

        if ((_bits & 16777216UL) != 0UL)
        {
            _r.ContentMD5 = default;
        }

        if ((_bits & 33554432UL) != 0UL)
        {
            _r.Expect = default;
        }

        if ((_bits & 67108864UL) != 0UL)
        {
            _r.IfMatch = default;
        }

        if ((_bits & 134217728UL) != 0UL)
        {
            _r.IfModifiedSince = default;
        }

        if ((_bits & 268435456UL) != 0UL)
        {
            _r.IfNoneMatch = default;
        }

        if ((_bits & 536870912UL) != 0UL)
        {
            _r.IfRange = default;
        }

        if ((_bits & 1073741824UL) != 0UL)
        {
            _r.IfUnmodifiedSince = default;
        }

        if ((_bits & 2147483648UL) != 0UL)
        {
            _r.MaxForwards = default;
        }

        if ((_bits & 4294967296UL) != 0UL)
        {
            _r.Pragma = default;
        }

        if ((_bits & 8589934592UL) != 0UL)
        {
            _r.Prefer = default;
        }

        if ((_bits & 17179869184UL) != 0UL)
        {
            _r.ProxyAuthorization = default;
        }

        if ((_bits & 34359738368UL) != 0UL)
        {
            _r.Range = default;
        }

        if ((_bits & 68719476736UL) != 0UL)
        {
            _r.Referer = default;
        }

        if ((_bits & 137438953472UL) != 0UL)
        {
            _r.TE = default;
        }

        if ((_bits & 274877906944UL) != 0UL)
        {
            _r.Trailer = default;
        }

        if ((_bits & 549755813888UL) != 0UL)
        {
            _r.TransferEncoding = default;
        }

        if ((_bits & 1099511627776UL) != 0UL)
        {
            _r.ProxyConnection = default;
        }

        if ((_bits & 2199023255552UL) != 0UL)
        {
            _r.XCorrelationID = default;
        }

        if ((_bits & 4398046511104UL) != 0UL)
        {
            _r.CorrelationID = default;
        }

        if ((_bits & 8796093022208UL) != 0UL)
        {
            _r.RequestId = default;
        }

        if ((_bits & 17592186044416UL) != 0UL)
        {
            _r.KeepAlive = default;
        }

        if ((_bits & 35184372088832UL) != 0UL)
        {
            _r.ProxyAuthenticate = default;
        }

        if ((_bits & 70368744177664UL) != 0UL)
        {
            _r.Forwarded = default;
        }

        if ((_bits & 140737488355328UL) != 0UL)
        {
            _r.XCsrfToken = default;
        }
    }

    private bool FastAdd(KnownHeaderType k, StringValues value)
    {
        switch (k)
        {
            case KnownHeaderType.Host:
                Host = value;
                return true;

            case KnownHeaderType.Connection:
                Connection = value;
                return true;

            case KnownHeaderType.ContentLength:
                _bits |= 4UL;
                _contentLength = ParseContentLength(value.ToString());
                return true;

            case KnownHeaderType.UserAgent:
                UserAgent = value;
                return true;

            case KnownHeaderType.Upgrade:
                Upgrade = value;
                return true;

            case KnownHeaderType.UpgradeInsecureRequests:
                UpgradeInsecureRequests = value;
                return true;

            case KnownHeaderType.Cookie:
                Cookie = value;
                return true;

            case KnownHeaderType.TraceParent:
                TraceParent = value;
                return true;

            case KnownHeaderType.TraceState:
                TraceState = value;
                return true;

            case KnownHeaderType.XForwardedFor:
                XForwardedFor = value;
                return true;

            case KnownHeaderType.XForwardedHost:
                XForwardedHost = value;
                return true;

            case KnownHeaderType.XForwardedProto:
                XForwardedProto = value;
                return true;

            case KnownHeaderType.Origin:
                Origin = value;
                return true;

            case KnownHeaderType.CacheControl:
                CacheControl = value;
                return true;

            case KnownHeaderType.ContentType:
                ContentType = value;
                return true;

            case KnownHeaderType.AccessControlRequestMethod:
                AccessControlRequestMethod = value;
                return true;

            case KnownHeaderType.AccessControlRequestHeaders:
                AccessControlRequestHeaders = value;
                return true;

            case KnownHeaderType.XRequestID:
                XRequestID = value;
                return true;

            case KnownHeaderType.Accept:
                Accept = value;
                return true;

            case KnownHeaderType.AcceptCharset:
                AcceptCharset = value;
                return true;

            case KnownHeaderType.AcceptDatetime:
                AcceptDatetime = value;
                return true;

            case KnownHeaderType.AcceptEncoding:
                AcceptEncoding = value;
                return true;

            case KnownHeaderType.AcceptLanguage:
                AcceptLanguage = value;
                return true;

            case KnownHeaderType.ContentEncoding:
                ContentEncoding = value;
                return true;

            case KnownHeaderType.ContentMD5:
                ContentMD5 = value;
                return true;

            case KnownHeaderType.Expect:
                Expect = value;
                return true;

            case KnownHeaderType.IfMatch:
                IfMatch = value;
                return true;

            case KnownHeaderType.IfModifiedSince:
                IfModifiedSince = value;
                return true;

            case KnownHeaderType.IfNoneMatch:
                IfNoneMatch = value;
                return true;

            case KnownHeaderType.IfRange:
                IfRange = value;
                return true;

            case KnownHeaderType.IfUnmodifiedSince:
                IfUnmodifiedSince = value;
                return true;

            case KnownHeaderType.MaxForwards:
                MaxForwards = value;
                return true;

            case KnownHeaderType.Pragma:
                Pragma = value;
                return true;

            case KnownHeaderType.Prefer:
                Prefer = value;
                return true;

            case KnownHeaderType.ProxyAuthorization:
                ProxyAuthorization = value;
                return true;

            case KnownHeaderType.Range:
                Range = value;
                return true;

            case KnownHeaderType.Referer:
                Referer = value;
                return true;

            case KnownHeaderType.TE:
                TE = value;
                return true;

            case KnownHeaderType.Trailer:
                Trailer = value;
                return true;

            case KnownHeaderType.TransferEncoding:
                TransferEncoding = value;
                return true;

            case KnownHeaderType.ProxyConnection:
                ProxyConnection = value;
                return true;

            case KnownHeaderType.XCorrelationID:
                XCorrelationID = value;
                return true;

            case KnownHeaderType.CorrelationID:
                CorrelationID = value;
                return true;

            case KnownHeaderType.RequestId:
                RequestId = value;
                return true;

            case KnownHeaderType.KeepAlive:
                KeepAlive = value;
                return true;

            case KnownHeaderType.ProxyAuthenticate:
                ProxyAuthenticate = value;
                return true;

            case KnownHeaderType.Forwarded:
                Forwarded = value;
                return true;

            case KnownHeaderType.XCsrfToken:
                XCsrfToken = value;
                return true;

            case KnownHeaderType.Unknown:
            default:
                return false;
        }
    }

    private bool FastRemove(KnownHeaderType k)
    {
        switch (k)
        {
            case KnownHeaderType.Host:
                if ((_bits & 1UL) != 0UL)
                {
                    _bits &= ~1UL;
                    _r.Host = default;
                    return true;
                }
                return false;

            case KnownHeaderType.Connection:
                if ((_bits & 2UL) != 0UL)
                {
                    _bits &= ~2UL;
                    _r.Connection = default;
                    return true;
                }
                return false;

            case KnownHeaderType.ContentLength:
                if ((_bits & 4UL) != 0UL)
                {
                    _bits &= ~4UL;
                    _contentLength = default;
                    return true;
                }
                return true;

            case KnownHeaderType.UserAgent:
                if ((_bits & 8UL) != 0UL)
                {
                    _bits &= ~8UL;
                    _r.UserAgent = default;
                    return true;
                }
                return false;

            case KnownHeaderType.Upgrade:
                if ((_bits & 16UL) != 0UL)
                {
                    _bits &= ~16UL;
                    _r.Upgrade = default;
                    return true;
                }
                return false;

            case KnownHeaderType.UpgradeInsecureRequests:
                if ((_bits & 32UL) != 0UL)
                {
                    _bits &= ~32UL;
                    _r.UpgradeInsecureRequests = default;
                    return true;
                }
                return false;

            case KnownHeaderType.Cookie:
                if ((_bits & 64UL) != 0UL)
                {
                    _bits &= ~64UL;
                    _r.Cookie = default;
                    return true;
                }
                return false;

            case KnownHeaderType.TraceParent:
                if ((_bits & 128UL) != 0UL)
                {
                    _bits &= ~128UL;
                    _r.TraceParent = default;
                    return true;
                }
                return false;

            case KnownHeaderType.TraceState:
                if ((_bits & 256UL) != 0UL)
                {
                    _bits &= ~256UL;
                    _r.TraceState = default;
                    return true;
                }
                return false;

            case KnownHeaderType.XForwardedFor:
                if ((_bits & 512UL) != 0UL)
                {
                    _bits &= ~512UL;
                    _r.XForwardedFor = default;
                    return true;
                }
                return false;

            case KnownHeaderType.XForwardedHost:
                if ((_bits & 1024UL) != 0UL)
                {
                    _bits &= ~1024UL;
                    _r.XForwardedHost = default;
                    return true;
                }
                return false;

            case KnownHeaderType.XForwardedProto:
                if ((_bits & 2048UL) != 0UL)
                {
                    _bits &= ~2048UL;
                    _r.XForwardedProto = default;
                    return true;
                }
                return false;

            case KnownHeaderType.Origin:
                if ((_bits & 4096UL) != 0UL)
                {
                    _bits &= ~4096UL;
                    _r.Origin = default;
                    return true;
                }
                return false;

            case KnownHeaderType.CacheControl:
                if ((_bits & 8192UL) != 0UL)
                {
                    _bits &= ~8192UL;
                    _r.CacheControl = default;
                    return true;
                }
                return false;

            case KnownHeaderType.ContentType:
                if ((_bits & 16384UL) != 0UL)
                {
                    _bits &= ~16384UL;
                    _r.ContentType = default;
                    return true;
                }
                return false;

            case KnownHeaderType.AccessControlRequestMethod:
                if ((_bits & 32768UL) != 0UL)
                {
                    _bits &= ~32768UL;
                    _r.AccessControlRequestMethod = default;
                    return true;
                }
                return false;

            case KnownHeaderType.AccessControlRequestHeaders:
                if ((_bits & 65536UL) != 0UL)
                {
                    _bits &= ~65536UL;
                    _r.AccessControlRequestHeaders = default;
                    return true;
                }
                return false;

            case KnownHeaderType.XRequestID:
                if ((_bits & 131072UL) != 0UL)
                {
                    _bits &= ~131072UL;
                    _r.XRequestID = default;
                    return true;
                }
                return false;

            case KnownHeaderType.Accept:
                if ((_bits & 262144UL) != 0UL)
                {
                    _bits &= ~262144UL;
                    _r.Accept = default;
                    return true;
                }
                return false;

            case KnownHeaderType.AcceptCharset:
                if ((_bits & 524288UL) != 0UL)
                {
                    _bits &= ~524288UL;
                    _r.AcceptCharset = default;
                    return true;
                }
                return false;

            case KnownHeaderType.AcceptDatetime:
                if ((_bits & 1048576UL) != 0UL)
                {
                    _bits &= ~1048576UL;
                    _r.AcceptDatetime = default;
                    return true;
                }
                return false;

            case KnownHeaderType.AcceptEncoding:
                if ((_bits & 2097152UL) != 0UL)
                {
                    _bits &= ~2097152UL;
                    _r.AcceptEncoding = default;
                    return true;
                }
                return false;

            case KnownHeaderType.AcceptLanguage:
                if ((_bits & 4194304UL) != 0UL)
                {
                    _bits &= ~4194304UL;
                    _r.AcceptLanguage = default;
                    return true;
                }
                return false;

            case KnownHeaderType.ContentEncoding:
                if ((_bits & 8388608UL) != 0UL)
                {
                    _bits &= ~8388608UL;
                    _r.ContentEncoding = default;
                    return true;
                }
                return false;

            case KnownHeaderType.ContentMD5:
                if ((_bits & 16777216UL) != 0UL)
                {
                    _bits &= ~16777216UL;
                    _r.ContentMD5 = default;
                    return true;
                }
                return false;

            case KnownHeaderType.Expect:
                if ((_bits & 33554432UL) != 0UL)
                {
                    _bits &= ~33554432UL;
                    _r.Expect = default;
                    return true;
                }
                return false;

            case KnownHeaderType.IfMatch:
                if ((_bits & 67108864UL) != 0UL)
                {
                    _bits &= ~67108864UL;
                    _r.IfMatch = default;
                    return true;
                }
                return false;

            case KnownHeaderType.IfModifiedSince:
                if ((_bits & 134217728UL) != 0UL)
                {
                    _bits &= ~134217728UL;
                    _r.IfModifiedSince = default;
                    return true;
                }
                return false;

            case KnownHeaderType.IfNoneMatch:
                if ((_bits & 268435456UL) != 0UL)
                {
                    _bits &= ~268435456UL;
                    _r.IfNoneMatch = default;
                    return true;
                }
                return false;

            case KnownHeaderType.IfRange:
                if ((_bits & 536870912UL) != 0UL)
                {
                    _bits &= ~536870912UL;
                    _r.IfRange = default;
                    return true;
                }
                return false;

            case KnownHeaderType.IfUnmodifiedSince:
                if ((_bits & 1073741824UL) != 0UL)
                {
                    _bits &= ~1073741824UL;
                    _r.IfUnmodifiedSince = default;
                    return true;
                }
                return false;

            case KnownHeaderType.MaxForwards:
                if ((_bits & 2147483648UL) != 0UL)
                {
                    _bits &= ~2147483648UL;
                    _r.MaxForwards = default;
                    return true;
                }
                return false;

            case KnownHeaderType.Pragma:
                if ((_bits & 4294967296UL) != 0UL)
                {
                    _bits &= ~4294967296UL;
                    _r.Pragma = default;
                    return true;
                }
                return false;

            case KnownHeaderType.Prefer:
                if ((_bits & 8589934592UL) != 0UL)
                {
                    _bits &= ~8589934592UL;
                    _r.Prefer = default;
                    return true;
                }
                return false;

            case KnownHeaderType.ProxyAuthorization:
                if ((_bits & 17179869184UL) != 0UL)
                {
                    _bits &= ~17179869184UL;
                    _r.ProxyAuthorization = default;
                    return true;
                }
                return false;

            case KnownHeaderType.Range:
                if ((_bits & 34359738368UL) != 0UL)
                {
                    _bits &= ~34359738368UL;
                    _r.Range = default;
                    return true;
                }
                return false;

            case KnownHeaderType.Referer:
                if ((_bits & 68719476736UL) != 0UL)
                {
                    _bits &= ~68719476736UL;
                    _r.Referer = default;
                    return true;
                }
                return false;

            case KnownHeaderType.TE:
                if ((_bits & 137438953472UL) != 0UL)
                {
                    _bits &= ~137438953472UL;
                    _r.TE = default;
                    return true;
                }
                return false;

            case KnownHeaderType.Trailer:
                if ((_bits & 274877906944UL) != 0UL)
                {
                    _bits &= ~274877906944UL;
                    _r.Trailer = default;
                    return true;
                }
                return false;

            case KnownHeaderType.TransferEncoding:
                if ((_bits & 549755813888UL) != 0UL)
                {
                    _bits &= ~549755813888UL;
                    _r.TransferEncoding = default;
                    return true;
                }
                return false;

            case KnownHeaderType.ProxyConnection:
                if ((_bits & 1099511627776UL) != 0UL)
                {
                    _bits &= ~1099511627776UL;
                    _r.ProxyConnection = default;
                    return true;
                }
                return false;

            case KnownHeaderType.XCorrelationID:
                if ((_bits & 2199023255552UL) != 0UL)
                {
                    _bits &= ~2199023255552UL;
                    _r.XCorrelationID = default;
                    return true;
                }
                return false;

            case KnownHeaderType.CorrelationID:
                if ((_bits & 4398046511104UL) != 0UL)
                {
                    _bits &= ~4398046511104UL;
                    _r.CorrelationID = default;
                    return true;
                }
                return false;

            case KnownHeaderType.RequestId:
                if ((_bits & 8796093022208UL) != 0UL)
                {
                    _bits &= ~8796093022208UL;
                    _r.RequestId = default;
                    return true;
                }
                return false;

            case KnownHeaderType.KeepAlive:
                if ((_bits & 17592186044416UL) != 0UL)
                {
                    _bits &= ~17592186044416UL;
                    _r.KeepAlive = default;
                    return true;
                }
                return false;

            case KnownHeaderType.ProxyAuthenticate:
                if ((_bits & 35184372088832UL) != 0UL)
                {
                    _bits &= ~35184372088832UL;
                    _r.ProxyAuthenticate = default;
                    return true;
                }
                return false;

            case KnownHeaderType.Forwarded:
                if ((_bits & 70368744177664UL) != 0UL)
                {
                    _bits &= ~70368744177664UL;
                    _r.Forwarded = default;
                    return true;
                }
                return false;

            case KnownHeaderType.XCsrfToken:
                if ((_bits & 140737488355328UL) != 0UL)
                {
                    _bits &= ~140737488355328UL;
                    _r.XCsrfToken = default;
                    return true;
                }
                return false;

            case KnownHeaderType.Unknown:
            default:
                return false;
        }
    }

    private bool FastTryGetValue(KnownHeaderType k, out StringValues value)
    {
        switch (k)
        {
            case KnownHeaderType.Host:
                if ((_bits & 1UL) != 0UL)
                {
                    value = _r.Host;
                    return true;
                }
                break;

            case KnownHeaderType.Connection:
                if ((_bits & 2UL) != 0UL)
                {
                    value = _r.Connection;
                    return true;
                }
                break;

            case KnownHeaderType.ContentLength:
                if ((_bits & 4UL) != 0UL)
                {
                    value = HeaderUtilities.FormatNonNegativeInt64(_contentLength.Value);
                    return true;
                }
                break;

            case KnownHeaderType.UserAgent:
                if ((_bits & 8UL) != 0UL)
                {
                    value = _r.UserAgent;
                    return true;
                }
                break;

            case KnownHeaderType.Upgrade:
                if ((_bits & 16UL) != 0UL)
                {
                    value = _r.Upgrade;
                    return true;
                }
                break;

            case KnownHeaderType.UpgradeInsecureRequests:
                if ((_bits & 32UL) != 0UL)
                {
                    value = _r.UpgradeInsecureRequests;
                    return true;
                }
                break;

            case KnownHeaderType.Cookie:
                if ((_bits & 64UL) != 0UL)
                {
                    value = _r.Cookie;
                    return true;
                }
                break;

            case KnownHeaderType.TraceParent:
                if ((_bits & 128UL) != 0UL)
                {
                    value = _r.TraceParent;
                    return true;
                }
                break;

            case KnownHeaderType.TraceState:
                if ((_bits & 256UL) != 0UL)
                {
                    value = _r.TraceState;
                    return true;
                }
                break;

            case KnownHeaderType.XForwardedFor:
                if ((_bits & 512UL) != 0UL)
                {
                    value = _r.XForwardedFor;
                    return true;
                }
                break;

            case KnownHeaderType.XForwardedHost:
                if ((_bits & 1024UL) != 0UL)
                {
                    value = _r.XForwardedHost;
                    return true;
                }
                break;

            case KnownHeaderType.XForwardedProto:
                if ((_bits & 2048UL) != 0UL)
                {
                    value = _r.XForwardedProto;
                    return true;
                }
                break;

            case KnownHeaderType.Origin:
                if ((_bits & 4096UL) != 0UL)
                {
                    value = _r.Origin;
                    return true;
                }
                break;

            case KnownHeaderType.CacheControl:
                if ((_bits & 8192UL) != 0UL)
                {
                    value = _r.CacheControl;
                    return true;
                }
                break;

            case KnownHeaderType.ContentType:
                if ((_bits & 16384UL) != 0UL)
                {
                    value = _r.ContentType;
                    return true;
                }
                break;

            case KnownHeaderType.AccessControlRequestMethod:
                if ((_bits & 32768UL) != 0UL)
                {
                    value = _r.AccessControlRequestMethod;
                    return true;
                }
                break;

            case KnownHeaderType.AccessControlRequestHeaders:
                if ((_bits & 65536UL) != 0UL)
                {
                    value = _r.AccessControlRequestHeaders;
                    return true;
                }
                break;

            case KnownHeaderType.XRequestID:
                if ((_bits & 131072UL) != 0UL)
                {
                    value = _r.XRequestID;
                    return true;
                }
                break;

            case KnownHeaderType.Accept:
                if ((_bits & 262144UL) != 0UL)
                {
                    value = _r.Accept;
                    return true;
                }
                break;

            case KnownHeaderType.AcceptCharset:
                if ((_bits & 524288UL) != 0UL)
                {
                    value = _r.AcceptCharset;
                    return true;
                }
                break;

            case KnownHeaderType.AcceptDatetime:
                if ((_bits & 1048576UL) != 0UL)
                {
                    value = _r.AcceptDatetime;
                    return true;
                }
                break;

            case KnownHeaderType.AcceptEncoding:
                if ((_bits & 2097152UL) != 0UL)
                {
                    value = _r.AcceptEncoding;
                    return true;
                }
                break;

            case KnownHeaderType.AcceptLanguage:
                if ((_bits & 4194304UL) != 0UL)
                {
                    value = _r.AcceptLanguage;
                    return true;
                }
                break;

            case KnownHeaderType.ContentEncoding:
                if ((_bits & 8388608UL) != 0UL)
                {
                    value = _r.ContentEncoding;
                    return true;
                }
                break;

            case KnownHeaderType.ContentMD5:
                if ((_bits & 16777216UL) != 0UL)
                {
                    value = _r.ContentMD5;
                    return true;
                }
                break;

            case KnownHeaderType.Expect:
                if ((_bits & 33554432UL) != 0UL)
                {
                    value = _r.Expect;
                    return true;
                }
                break;

            case KnownHeaderType.IfMatch:
                if ((_bits & 67108864UL) != 0UL)
                {
                    value = _r.IfMatch;
                    return true;
                }
                break;

            case KnownHeaderType.IfModifiedSince:
                if ((_bits & 134217728UL) != 0UL)
                {
                    value = _r.IfModifiedSince;
                    return true;
                }
                break;

            case KnownHeaderType.IfNoneMatch:
                if ((_bits & 268435456UL) != 0UL)
                {
                    value = _r.IfNoneMatch;
                    return true;
                }
                break;

            case KnownHeaderType.IfRange:
                if ((_bits & 536870912UL) != 0UL)
                {
                    value = _r.IfRange;
                    return true;
                }
                break;

            case KnownHeaderType.IfUnmodifiedSince:
                if ((_bits & 1073741824UL) != 0UL)
                {
                    value = _r.IfUnmodifiedSince;
                    return true;
                }
                break;

            case KnownHeaderType.MaxForwards:
                if ((_bits & 2147483648UL) != 0UL)
                {
                    value = _r.MaxForwards;
                    return true;
                }
                break;

            case KnownHeaderType.Pragma:
                if ((_bits & 4294967296UL) != 0UL)
                {
                    value = _r.Pragma;
                    return true;
                }
                break;

            case KnownHeaderType.Prefer:
                if ((_bits & 8589934592UL) != 0UL)
                {
                    value = _r.Prefer;
                    return true;
                }
                break;

            case KnownHeaderType.ProxyAuthorization:
                if ((_bits & 17179869184UL) != 0UL)
                {
                    value = _r.ProxyAuthorization;
                    return true;
                }
                break;

            case KnownHeaderType.Range:
                if ((_bits & 34359738368UL) != 0UL)
                {
                    value = _r.Range;
                    return true;
                }
                break;

            case KnownHeaderType.Referer:
                if ((_bits & 68719476736UL) != 0UL)
                {
                    value = _r.Referer;
                    return true;
                }
                break;

            case KnownHeaderType.TE:
                if ((_bits & 137438953472UL) != 0UL)
                {
                    value = _r.TE;
                    return true;
                }
                break;

            case KnownHeaderType.Trailer:
                if ((_bits & 274877906944UL) != 0UL)
                {
                    value = _r.Trailer;
                    return true;
                }
                break;

            case KnownHeaderType.TransferEncoding:
                if ((_bits & 549755813888UL) != 0UL)
                {
                    value = _r.TransferEncoding;
                    return true;
                }
                break;

            case KnownHeaderType.ProxyConnection:
                if ((_bits & 1099511627776UL) != 0UL)
                {
                    value = _r.ProxyConnection;
                    return true;
                }
                break;

            case KnownHeaderType.XCorrelationID:
                if ((_bits & 2199023255552UL) != 0UL)
                {
                    value = _r.XCorrelationID;
                    return true;
                }
                break;

            case KnownHeaderType.CorrelationID:
                if ((_bits & 4398046511104UL) != 0UL)
                {
                    value = _r.CorrelationID;
                    return true;
                }
                break;

            case KnownHeaderType.RequestId:
                if ((_bits & 8796093022208UL) != 0UL)
                {
                    value = _r.RequestId;
                    return true;
                }
                break;

            case KnownHeaderType.KeepAlive:
                if ((_bits & 17592186044416UL) != 0UL)
                {
                    value = _r.KeepAlive;
                    return true;
                }
                break;

            case KnownHeaderType.ProxyAuthenticate:
                if ((_bits & 35184372088832UL) != 0UL)
                {
                    value = _r.ProxyAuthenticate;
                    return true;
                }
                break;

            case KnownHeaderType.Forwarded:
                if ((_bits & 70368744177664UL) != 0UL)
                {
                    value = _r.Forwarded;
                    return true;
                }
                break;

            case KnownHeaderType.XCsrfToken:
                if ((_bits & 140737488355328UL) != 0UL)
                {
                    value = _r.XCsrfToken;
                    return true;
                }
                break;

            case KnownHeaderType.Unknown:
            default:
                break;
        }
        value = default;
        return false;
    }

    public void CopyTo(KeyValuePair<string, StringValues>[] array, int arrayIndex)
    {
        if (arrayIndex < 0)
        {
            return;
        }

        if ((_bits & 1UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.Host, _r.Host);
            ++arrayIndex;
        }

        if ((_bits & 2UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.Connection, _r.Connection);
            ++arrayIndex;
        }

        if ((_bits & 4UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.ContentLength, HeaderUtilities.FormatNonNegativeInt64(_contentLength.Value));
            ++arrayIndex;
        }

        if ((_bits & 8UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.UserAgent, _r.UserAgent);
            ++arrayIndex;
        }

        if ((_bits & 16UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.Upgrade, _r.Upgrade);
            ++arrayIndex;
        }

        if ((_bits & 32UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.UpgradeInsecureRequests, _r.UpgradeInsecureRequests);
            ++arrayIndex;
        }

        if ((_bits & 64UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.Cookie, _r.Cookie);
            ++arrayIndex;
        }

        if ((_bits & 128UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.TraceParent, _r.TraceParent);
            ++arrayIndex;
        }

        if ((_bits & 256UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.TraceState, _r.TraceState);
            ++arrayIndex;
        }

        if ((_bits & 512UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.XForwardedFor, _r.XForwardedFor);
            ++arrayIndex;
        }

        if ((_bits & 1024UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.XForwardedHost, _r.XForwardedHost);
            ++arrayIndex;
        }

        if ((_bits & 2048UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.XForwardedProto, _r.XForwardedProto);
            ++arrayIndex;
        }

        if ((_bits & 4096UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.Origin, _r.Origin);
            ++arrayIndex;
        }

        if ((_bits & 8192UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.CacheControl, _r.CacheControl);
            ++arrayIndex;
        }

        if ((_bits & 16384UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.ContentType, _r.ContentType);
            ++arrayIndex;
        }

        if ((_bits & 32768UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.AccessControlRequestMethod, _r.AccessControlRequestMethod);
            ++arrayIndex;
        }

        if ((_bits & 65536UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.AccessControlRequestHeaders, _r.AccessControlRequestHeaders);
            ++arrayIndex;
        }

        if ((_bits & 131072UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.XRequestID, _r.XRequestID);
            ++arrayIndex;
        }

        if ((_bits & 262144UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.Accept, _r.Accept);
            ++arrayIndex;
        }

        if ((_bits & 524288UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.AcceptCharset, _r.AcceptCharset);
            ++arrayIndex;
        }

        if ((_bits & 1048576UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.AcceptDatetime, _r.AcceptDatetime);
            ++arrayIndex;
        }

        if ((_bits & 2097152UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.AcceptEncoding, _r.AcceptEncoding);
            ++arrayIndex;
        }

        if ((_bits & 4194304UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.AcceptLanguage, _r.AcceptLanguage);
            ++arrayIndex;
        }

        if ((_bits & 8388608UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.ContentEncoding, _r.ContentEncoding);
            ++arrayIndex;
        }

        if ((_bits & 16777216UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.ContentMD5, _r.ContentMD5);
            ++arrayIndex;
        }

        if ((_bits & 33554432UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.Expect, _r.Expect);
            ++arrayIndex;
        }

        if ((_bits & 67108864UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.IfMatch, _r.IfMatch);
            ++arrayIndex;
        }

        if ((_bits & 134217728UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.IfModifiedSince, _r.IfModifiedSince);
            ++arrayIndex;
        }

        if ((_bits & 268435456UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.IfNoneMatch, _r.IfNoneMatch);
            ++arrayIndex;
        }

        if ((_bits & 536870912UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.IfRange, _r.IfRange);
            ++arrayIndex;
        }

        if ((_bits & 1073741824UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.IfUnmodifiedSince, _r.IfUnmodifiedSince);
            ++arrayIndex;
        }

        if ((_bits & 2147483648UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.MaxForwards, _r.MaxForwards);
            ++arrayIndex;
        }

        if ((_bits & 4294967296UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.Pragma, _r.Pragma);
            ++arrayIndex;
        }

        if ((_bits & 8589934592UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.Prefer, _r.Prefer);
            ++arrayIndex;
        }

        if ((_bits & 17179869184UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.ProxyAuthorization, _r.ProxyAuthorization);
            ++arrayIndex;
        }

        if ((_bits & 34359738368UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.Range, _r.Range);
            ++arrayIndex;
        }

        if ((_bits & 68719476736UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.Referer, _r.Referer);
            ++arrayIndex;
        }

        if ((_bits & 137438953472UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.TE, _r.TE);
            ++arrayIndex;
        }

        if ((_bits & 274877906944UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.Trailer, _r.Trailer);
            ++arrayIndex;
        }

        if ((_bits & 549755813888UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.TransferEncoding, _r.TransferEncoding);
            ++arrayIndex;
        }

        if ((_bits & 1099511627776UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.ProxyConnection, _r.ProxyConnection);
            ++arrayIndex;
        }

        if ((_bits & 2199023255552UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.XCorrelationID, _r.XCorrelationID);
            ++arrayIndex;
        }

        if ((_bits & 4398046511104UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.CorrelationID, _r.CorrelationID);
            ++arrayIndex;
        }

        if ((_bits & 8796093022208UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.RequestId, _r.RequestId);
            ++arrayIndex;
        }

        if ((_bits & 17592186044416UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.KeepAlive, _r.KeepAlive);
            ++arrayIndex;
        }

        if ((_bits & 35184372088832UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.ProxyAuthenticate, _r.ProxyAuthenticate);
            ++arrayIndex;
        }

        if ((_bits & 70368744177664UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.Forwarded, _r.Forwarded);
            ++arrayIndex;
        }

        if ((_bits & 140737488355328UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.XCsrfToken, _r.XCsrfToken);
            ++arrayIndex;
        }

        ((ICollection<KeyValuePair<string, StringValues>>?)dict)?.CopyTo(array, arrayIndex);
    }

    public partial struct Enumerator
    {
        public bool MoveNext()
        {
            switch (_next)
            {
                case 0:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.Host, _collection._r.Host);
                    _currentBits &= ~1UL;
                    break;

                case 1:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.Connection, _collection._r.Connection);
                    _currentBits &= ~2UL;
                    break;

                case 2:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.ContentLength, HeaderUtilities.FormatNonNegativeInt64(_collection._contentLength.Value));
                    _currentBits &= ~4UL;
                    break;

                case 3:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.UserAgent, _collection._r.UserAgent);
                    _currentBits &= ~8UL;
                    break;

                case 4:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.Upgrade, _collection._r.Upgrade);
                    _currentBits &= ~16UL;
                    break;

                case 5:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.UpgradeInsecureRequests, _collection._r.UpgradeInsecureRequests);
                    _currentBits &= ~32UL;
                    break;

                case 6:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.Cookie, _collection._r.Cookie);
                    _currentBits &= ~64UL;
                    break;

                case 7:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.TraceParent, _collection._r.TraceParent);
                    _currentBits &= ~128UL;
                    break;

                case 8:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.TraceState, _collection._r.TraceState);
                    _currentBits &= ~256UL;
                    break;

                case 9:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.XForwardedFor, _collection._r.XForwardedFor);
                    _currentBits &= ~512UL;
                    break;

                case 10:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.XForwardedHost, _collection._r.XForwardedHost);
                    _currentBits &= ~1024UL;
                    break;

                case 11:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.XForwardedProto, _collection._r.XForwardedProto);
                    _currentBits &= ~2048UL;
                    break;

                case 12:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.Origin, _collection._r.Origin);
                    _currentBits &= ~4096UL;
                    break;

                case 13:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.CacheControl, _collection._r.CacheControl);
                    _currentBits &= ~8192UL;
                    break;

                case 14:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.ContentType, _collection._r.ContentType);
                    _currentBits &= ~16384UL;
                    break;

                case 15:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.AccessControlRequestMethod, _collection._r.AccessControlRequestMethod);
                    _currentBits &= ~32768UL;
                    break;

                case 16:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.AccessControlRequestHeaders, _collection._r.AccessControlRequestHeaders);
                    _currentBits &= ~65536UL;
                    break;

                case 17:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.XRequestID, _collection._r.XRequestID);
                    _currentBits &= ~131072UL;
                    break;

                case 18:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.Accept, _collection._r.Accept);
                    _currentBits &= ~262144UL;
                    break;

                case 19:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.AcceptCharset, _collection._r.AcceptCharset);
                    _currentBits &= ~524288UL;
                    break;

                case 20:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.AcceptDatetime, _collection._r.AcceptDatetime);
                    _currentBits &= ~1048576UL;
                    break;

                case 21:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.AcceptEncoding, _collection._r.AcceptEncoding);
                    _currentBits &= ~2097152UL;
                    break;

                case 22:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.AcceptLanguage, _collection._r.AcceptLanguage);
                    _currentBits &= ~4194304UL;
                    break;

                case 23:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.ContentEncoding, _collection._r.ContentEncoding);
                    _currentBits &= ~8388608UL;
                    break;

                case 24:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.ContentMD5, _collection._r.ContentMD5);
                    _currentBits &= ~16777216UL;
                    break;

                case 25:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.Expect, _collection._r.Expect);
                    _currentBits &= ~33554432UL;
                    break;

                case 26:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.IfMatch, _collection._r.IfMatch);
                    _currentBits &= ~67108864UL;
                    break;

                case 27:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.IfModifiedSince, _collection._r.IfModifiedSince);
                    _currentBits &= ~134217728UL;
                    break;

                case 28:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.IfNoneMatch, _collection._r.IfNoneMatch);
                    _currentBits &= ~268435456UL;
                    break;

                case 29:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.IfRange, _collection._r.IfRange);
                    _currentBits &= ~536870912UL;
                    break;

                case 30:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.IfUnmodifiedSince, _collection._r.IfUnmodifiedSince);
                    _currentBits &= ~1073741824UL;
                    break;

                case 31:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.MaxForwards, _collection._r.MaxForwards);
                    _currentBits &= ~2147483648UL;
                    break;

                case 32:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.Pragma, _collection._r.Pragma);
                    _currentBits &= ~4294967296UL;
                    break;

                case 33:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.Prefer, _collection._r.Prefer);
                    _currentBits &= ~8589934592UL;
                    break;

                case 34:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.ProxyAuthorization, _collection._r.ProxyAuthorization);
                    _currentBits &= ~17179869184UL;
                    break;

                case 35:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.Range, _collection._r.Range);
                    _currentBits &= ~34359738368UL;
                    break;

                case 36:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.Referer, _collection._r.Referer);
                    _currentBits &= ~68719476736UL;
                    break;

                case 37:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.TE, _collection._r.TE);
                    _currentBits &= ~137438953472UL;
                    break;

                case 38:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.Trailer, _collection._r.Trailer);
                    _currentBits &= ~274877906944UL;
                    break;

                case 39:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.TransferEncoding, _collection._r.TransferEncoding);
                    _currentBits &= ~549755813888UL;
                    break;

                case 40:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.ProxyConnection, _collection._r.ProxyConnection);
                    _currentBits &= ~1099511627776UL;
                    break;

                case 41:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.XCorrelationID, _collection._r.XCorrelationID);
                    _currentBits &= ~2199023255552UL;
                    break;

                case 42:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.CorrelationID, _collection._r.CorrelationID);
                    _currentBits &= ~4398046511104UL;
                    break;

                case 43:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.RequestId, _collection._r.RequestId);
                    _currentBits &= ~8796093022208UL;
                    break;

                case 44:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.KeepAlive, _collection._r.KeepAlive);
                    _currentBits &= ~17592186044416UL;
                    break;

                case 45:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.ProxyAuthenticate, _collection._r.ProxyAuthenticate);
                    _currentBits &= ~35184372088832UL;
                    break;

                case 46:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.Forwarded, _collection._r.Forwarded);
                    _currentBits &= ~70368744177664UL;
                    break;

                case 47:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.XCsrfToken, _collection._r.XCsrfToken);
                    _currentBits &= ~140737488355328UL;
                    break;

                default:
                    if (_enumerator != null && _enumerator.MoveNext())
                    {
                        _current = _enumerator.Current;
                        return true;
                    }
                    return false;
            }

            if (_currentBits != 0)
            {
                _next = BitOperations.TrailingZeroCount(_currentBits);
                return true;
            }
            else
            {
                _next = -1;
                return true;
            }
        }
    }

    private struct HeaderReferences
    {
        public StringValues Host;
        public StringValues Connection;
        public StringValues UserAgent;
        public StringValues Upgrade;
        public StringValues UpgradeInsecureRequests;
        public StringValues Cookie;
        public StringValues TraceParent;
        public StringValues TraceState;
        public StringValues XForwardedFor;
        public StringValues XForwardedHost;
        public StringValues XForwardedProto;
        public StringValues Origin;
        public StringValues CacheControl;
        public StringValues ContentType;
        public StringValues AccessControlRequestMethod;
        public StringValues AccessControlRequestHeaders;
        public StringValues XRequestID;
        public StringValues Accept;
        public StringValues AcceptCharset;
        public StringValues AcceptDatetime;
        public StringValues AcceptEncoding;
        public StringValues AcceptLanguage;
        public StringValues ContentEncoding;
        public StringValues ContentMD5;
        public StringValues Expect;
        public StringValues IfMatch;
        public StringValues IfModifiedSince;
        public StringValues IfNoneMatch;
        public StringValues IfRange;
        public StringValues IfUnmodifiedSince;
        public StringValues MaxForwards;
        public StringValues Pragma;
        public StringValues Prefer;
        public StringValues ProxyAuthorization;
        public StringValues Range;
        public StringValues Referer;
        public StringValues TE;
        public StringValues Trailer;
        public StringValues TransferEncoding;
        public StringValues ProxyConnection;
        public StringValues XCorrelationID;
        public StringValues CorrelationID;
        public StringValues RequestId;
        public StringValues KeepAlive;
        public StringValues ProxyAuthenticate;
        public StringValues Forwarded;
        public StringValues XCsrfToken;
    }

    private enum KnownHeaderType
    {
        Unknown = 0,
        Host,
        Connection,
        ContentLength,
        UserAgent,
        Upgrade,
        UpgradeInsecureRequests,
        Cookie,
        TraceParent,
        TraceState,
        XForwardedFor,
        XForwardedHost,
        XForwardedProto,
        Origin,
        CacheControl,
        ContentType,
        AccessControlRequestMethod,
        AccessControlRequestHeaders,
        XRequestID,
        Accept,
        AcceptCharset,
        AcceptDatetime,
        AcceptEncoding,
        AcceptLanguage,
        ContentEncoding,
        ContentMD5,
        Expect,
        IfMatch,
        IfModifiedSince,
        IfNoneMatch,
        IfRange,
        IfUnmodifiedSince,
        MaxForwards,
        Pragma,
        Prefer,
        ProxyAuthorization,
        Range,
        Referer,
        TE,
        Trailer,
        TransferEncoding,
        ProxyConnection,
        XCorrelationID,
        CorrelationID,
        RequestId,
        KeepAlive,
        ProxyAuthenticate,
        Forwarded,
        XCsrfToken,
    }

    private static readonly FrozenDictionary<string, KnownHeaderType> _internedHeaderType = new Dictionary<string, KnownHeaderType>(StringComparer.OrdinalIgnoreCase)
    {
        {HeaderNames.Host,KnownHeaderType.Host},
{HeaderNames.Connection,KnownHeaderType.Connection},
{HeaderNames.ContentLength,KnownHeaderType.ContentLength},
{HeaderNames.UserAgent,KnownHeaderType.UserAgent},
{HeaderNames.Upgrade,KnownHeaderType.Upgrade},
{HeaderNames.UpgradeInsecureRequests,KnownHeaderType.UpgradeInsecureRequests},
{HeaderNames.Cookie,KnownHeaderType.Cookie},
{HeaderNames.TraceParent,KnownHeaderType.TraceParent},
{HeaderNames.TraceState,KnownHeaderType.TraceState},
{HeaderNames.XForwardedFor,KnownHeaderType.XForwardedFor},
{HeaderNames.XForwardedHost,KnownHeaderType.XForwardedHost},
{HeaderNames.XForwardedProto,KnownHeaderType.XForwardedProto},
{HeaderNames.Origin,KnownHeaderType.Origin},
{HeaderNames.CacheControl,KnownHeaderType.CacheControl},
{HeaderNames.ContentType,KnownHeaderType.ContentType},
{HeaderNames.AccessControlRequestMethod,KnownHeaderType.AccessControlRequestMethod},
{HeaderNames.AccessControlRequestHeaders,KnownHeaderType.AccessControlRequestHeaders},
{HeaderNames.XRequestID,KnownHeaderType.XRequestID},
{HeaderNames.Accept,KnownHeaderType.Accept},
{HeaderNames.AcceptCharset,KnownHeaderType.AcceptCharset},
{HeaderNames.AcceptDatetime,KnownHeaderType.AcceptDatetime},
{HeaderNames.AcceptEncoding,KnownHeaderType.AcceptEncoding},
{HeaderNames.AcceptLanguage,KnownHeaderType.AcceptLanguage},
{HeaderNames.ContentEncoding,KnownHeaderType.ContentEncoding},
{HeaderNames.ContentMD5,KnownHeaderType.ContentMD5},
{HeaderNames.Expect,KnownHeaderType.Expect},
{HeaderNames.IfMatch,KnownHeaderType.IfMatch},
{HeaderNames.IfModifiedSince,KnownHeaderType.IfModifiedSince},
{HeaderNames.IfNoneMatch,KnownHeaderType.IfNoneMatch},
{HeaderNames.IfRange,KnownHeaderType.IfRange},
{HeaderNames.IfUnmodifiedSince,KnownHeaderType.IfUnmodifiedSince},
{HeaderNames.MaxForwards,KnownHeaderType.MaxForwards},
{HeaderNames.Pragma,KnownHeaderType.Pragma},
{HeaderNames.Prefer,KnownHeaderType.Prefer},
{HeaderNames.ProxyAuthorization,KnownHeaderType.ProxyAuthorization},
{HeaderNames.Range,KnownHeaderType.Range},
{HeaderNames.Referer,KnownHeaderType.Referer},
{HeaderNames.TE,KnownHeaderType.TE},
{HeaderNames.Trailer,KnownHeaderType.Trailer},
{HeaderNames.TransferEncoding,KnownHeaderType.TransferEncoding},
{HeaderNames.ProxyConnection,KnownHeaderType.ProxyConnection},
{HeaderNames.XCorrelationID,KnownHeaderType.XCorrelationID},
{HeaderNames.CorrelationID,KnownHeaderType.CorrelationID},
{HeaderNames.RequestId,KnownHeaderType.RequestId},
{HeaderNames.KeepAlive,KnownHeaderType.KeepAlive},
{HeaderNames.ProxyAuthenticate,KnownHeaderType.ProxyAuthenticate},
{HeaderNames.Forwarded,KnownHeaderType.Forwarded},
{HeaderNames.XCsrfToken,KnownHeaderType.XCsrfToken},
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
}