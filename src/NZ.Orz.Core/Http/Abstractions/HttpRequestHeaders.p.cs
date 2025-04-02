using Microsoft.Extensions.Primitives;
using NZ.Orz.Http.Exceptions;
using System.Collections.Frozen;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

    public bool HasConnection => (_bits & 2UL) != 0;

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

    public bool HasCookie => (_bits & 64UL) != 0;

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

    public bool HasTransferEncoding => (_bits & 549755813888UL) != 0;

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

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void Append(ReadOnlySpan<byte> name, ReadOnlySpan<byte> value, bool checkForNewlineChars)
    {
        var nameStr = string.Empty;
        ref byte nameStart = ref MemoryMarshal.GetReference(name);
        ref StringValues values = ref Unsafe.NullRef<StringValues>();
        var flag = 0UL;
        switch (name.Length)
        {
            case 2:
                {
                    ref byte ns = ref nameStart;
                    var s0 = ReadUnalignedLittleEndian_ushort(ref ns);
                    if (s0 == 17748)
                    {
                        flag = 137438953472UL;
                        values = ref _r.TE;
                        nameStr = HeaderNames.TE;
                    }
                }
                break;

            case 4:
                {
                    ref byte ns = ref nameStart;
                    var i0 = ReadUnalignedLittleEndian_uint(ref ns);
                    if (i0 == 1953722184U)
                    {
                        flag = 1UL;
                        values = ref _r.Host;
                        nameStr = HeaderNames.Host;
                    }
                }
                break;

            case 5:
                {
                    ref byte ns = ref nameStart;
                    var i0 = ReadUnalignedLittleEndian_uint(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(4));
                    if (i0 == 1735287122U && ns == 101)
                    {
                        flag = 34359738368UL;
                        values = ref _r.Range;
                        nameStr = HeaderNames.Range;
                    }
                }
                break;

            case 6:
                {
                    ref byte ns = ref nameStart;
                    var i0 = ReadUnalignedLittleEndian_uint(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(4));
                    var s1 = ReadUnalignedLittleEndian_ushort(ref ns);
                    if (i0 == 1802465091U && s1 == 25961)
                    {
                        flag = 64UL;
                        values = ref _r.Cookie;
                        nameStr = HeaderNames.Cookie;
                    }
                    else if (i0 == 1734963791U && s1 == 28265)
                    {
                        flag = 4096UL;
                        values = ref _r.Origin;
                        nameStr = HeaderNames.Origin;
                    }
                    else if (i0 == 1701012289U && s1 == 29808)
                    {
                        flag = 262144UL;
                        values = ref _r.Accept;
                        nameStr = HeaderNames.Accept;
                    }
                    else if (i0 == 1701869637U && s1 == 29795)
                    {
                        flag = 33554432UL;
                        values = ref _r.Expect;
                        nameStr = HeaderNames.Expect;
                    }
                    else if (i0 == 1734439504U && s1 == 24941)
                    {
                        flag = 4294967296UL;
                        values = ref _r.Pragma;
                        nameStr = HeaderNames.Pragma;
                    }
                    else if (i0 == 1717924432U && s1 == 29285)
                    {
                        flag = 8589934592UL;
                        values = ref _r.Prefer;
                        nameStr = HeaderNames.Prefer;
                    }
                }
                break;

            case 7:
                {
                    ref byte ns = ref nameStart;
                    var i0 = ReadUnalignedLittleEndian_uint(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(4));
                    var s1 = ReadUnalignedLittleEndian_ushort(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(2));
                    if (i0 == 1919381589U && s1 == 25697 && ns == 101)
                    {
                        flag = 16UL;
                        values = ref _r.Upgrade;
                        nameStr = HeaderNames.Upgrade;
                    }
                    else if (i0 == 1701209426U && s1 == 25970 && ns == 114)
                    {
                        flag = 68719476736UL;
                        values = ref _r.Referer;
                        nameStr = HeaderNames.Referer;
                    }
                    else if (i0 == 1767993940U && s1 == 25964 && ns == 114)
                    {
                        flag = 274877906944UL;
                        values = ref _r.Trailer;
                        nameStr = HeaderNames.Trailer;
                    }
                }
                break;

            case 8:
                {
                    ref byte ns = ref nameStart;
                    var l0 = ReadUnalignedLittleEndian_ulong(ref ns);
                    if (l0 == 7521983763894330953UL)
                    {
                        flag = 67108864UL;
                        values = ref _r.IfMatch;
                        nameStr = HeaderNames.IfMatch;
                    }
                    else if (l0 == 7306930284701509193UL)
                    {
                        flag = 536870912UL;
                        values = ref _r.IfRange;
                        nameStr = HeaderNames.IfRange;
                    }
                }
                break;

            case 9:
                {
                    ref byte ns = ref nameStart;
                    var l0 = ReadUnalignedLittleEndian_ulong(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(8));
                    if (l0 == 7306090258443169606UL && ns == 100)
                    {
                        flag = 70368744177664UL;
                        values = ref _r.Forwarded;
                        nameStr = HeaderNames.Forwarded;
                    }
                }
                break;

            case 10:
                {
                    ref byte ns = ref nameStart;
                    var l0 = ReadUnalignedLittleEndian_ulong(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(8));
                    var s1 = ReadUnalignedLittleEndian_ushort(ref ns);
                    if (l0 == 7598807758576447299UL && s1 == 28271)
                    {
                        flag = 2UL;
                        values = ref _r.Connection;
                        nameStr = HeaderNames.Connection;
                    }
                    else if (l0 == 7306880583880504149UL && s1 == 29806)
                    {
                        flag = 8UL;
                        values = ref _r.UserAgent;
                        nameStr = HeaderNames.UserAgent;
                    }
                    else if (l0 == 7022364598273667700UL && s1 == 25972)
                    {
                        flag = 256UL;
                        values = ref _r.TraceState;
                        nameStr = HeaderNames.TraceState;
                    }
                    else if (l0 == 3275369708604450130UL && s1 == 25673)
                    {
                        flag = 8796093022208UL;
                        values = ref _r.RequestId;
                        nameStr = HeaderNames.RequestId;
                    }
                    else if (l0 == 7596518334882211147UL && s1 == 25974)
                    {
                        flag = 17592186044416UL;
                        values = ref _r.KeepAlive;
                        nameStr = HeaderNames.KeepAlive;
                    }
                }
                break;

            case 11:
                {
                    ref byte ns = ref nameStart;
                    var l0 = ReadUnalignedLittleEndian_ulong(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(8));
                    var s1 = ReadUnalignedLittleEndian_ushort(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(2));
                    if (l0 == 8241992373826056820UL && s1 == 28261 && ns == 116)
                    {
                        flag = 128UL;
                        values = ref _r.TraceParent;
                        nameStr = HeaderNames.TraceParent;
                    }
                    else if (l0 == 3275364211029339971UL && s1 == 17485 && ns == 53)
                    {
                        flag = 16777216UL;
                        values = ref _r.ContentMD5;
                        nameStr = HeaderNames.ContentMD5;
                    }
                }
                break;

            case 12:
                {
                    ref byte ns = ref nameStart;
                    var l0 = ReadUnalignedLittleEndian_ulong(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(8));
                    var i1 = ReadUnalignedLittleEndian_uint(ref ns);
                    if (l0 == 3275364211029339971UL && i1 == 1701869908U)
                    {
                        flag = 16384UL;
                        values = ref _r.ContentType;
                        nameStr = HeaderNames.ContentType;
                    }
                    else if (l0 == 8315181416901127512UL && i1 == 1145646452U)
                    {
                        flag = 131072UL;
                        values = ref _r.XRequestID;
                        nameStr = HeaderNames.XRequestID;
                    }
                    else if (l0 == 8607064185059696973UL && i1 == 1935962721U)
                    {
                        flag = 2147483648UL;
                        values = ref _r.MaxForwards;
                        nameStr = HeaderNames.MaxForwards;
                    }
                    else if (l0 == 6065616914884013400UL && i1 == 1852140399U)
                    {
                        flag = 140737488355328UL;
                        values = ref _r.XCsrfToken;
                        nameStr = HeaderNames.XCsrfToken;
                    }
                }
                break;

            case 13:
                {
                    ref byte ns = ref nameStart;
                    var l0 = ReadUnalignedLittleEndian_ulong(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(8));
                    var i1 = ReadUnalignedLittleEndian_uint(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(4));
                    if (l0 == 8017301675215905091UL && i1 == 1869771886U && ns == 108)
                    {
                        flag = 8192UL;
                        values = ref _r.CacheControl;
                        nameStr = HeaderNames.CacheControl;
                    }
                    else if (l0 == 3271142128686556745UL && i1 == 1668571469U && ns == 104)
                    {
                        flag = 268435456UL;
                        values = ref _r.IfNoneMatch;
                        nameStr = HeaderNames.IfNoneMatch;
                    }
                }
                break;

            case 14:
                {
                    ref byte ns = ref nameStart;
                    var l0 = ReadUnalignedLittleEndian_ulong(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(8));
                    var i1 = ReadUnalignedLittleEndian_uint(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(4));
                    var s2 = ReadUnalignedLittleEndian_ushort(ref ns);
                    if (l0 == 3275364211029339971UL && i1 == 1735288140U && s2 == 26740)
                    {
                        if ((_bits & 4UL) == 0)
                        {
                            _bits |= 4UL;
                            AppendContentLength(value);
                            return;
                        }
                        else
                        {
                            throw BadHttpRequestException.GetException(RequestRejectionReason.MultipleContentLengths);
                        }
                    }
                    else if (l0 == 4840653200579322689UL && i1 == 1936875880U && s2 == 29797)
                    {
                        flag = 524288UL;
                        values = ref _r.AcceptCharset;
                        nameStr = HeaderNames.AcceptCharset;
                    }
                    else if (l0 == 8386103164108173123UL && i1 == 762212201U && s2 == 17481)
                    {
                        flag = 4398046511104UL;
                        values = ref _r.CorrelationID;
                        nameStr = HeaderNames.CorrelationID;
                    }
                }
                break;

            case 15:
                {
                    ref byte ns = ref nameStart;
                    var l0 = ReadUnalignedLittleEndian_ulong(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(8));
                    var i1 = ReadUnalignedLittleEndian_uint(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(4));
                    var s2 = ReadUnalignedLittleEndian_ushort(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(2));
                    if (l0 == 8242000126441565528UL && i1 == 761554276U && s2 == 28486 && ns == 114)
                    {
                        flag = 512UL;
                        values = ref _r.XForwardedFor;
                        nameStr = HeaderNames.XForwardedFor;
                    }
                    else if (l0 == 4912710794617250625UL && i1 == 1952805985U && s2 == 28009 && ns == 101)
                    {
                        flag = 1048576UL;
                        values = ref _r.AcceptDatetime;
                        nameStr = HeaderNames.AcceptDatetime;
                    }
                    else if (l0 == 4984768388655178561UL && i1 == 1685021550U && s2 == 28265 && ns == 103)
                    {
                        flag = 2097152UL;
                        values = ref _r.AcceptEncoding;
                        nameStr = HeaderNames.AcceptEncoding;
                    }
                    else if (l0 == 5489171546920674113UL && i1 == 1969712737U && s2 == 26465 && ns == 101)
                    {
                        flag = 4194304UL;
                        values = ref _r.AcceptLanguage;
                        nameStr = HeaderNames.AcceptLanguage;
                    }
                }
                break;

            case 16:
                {
                    ref byte ns = ref nameStart;
                    var l0 = ReadUnalignedLittleEndian_ulong(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(8));
                    var l1 = ReadUnalignedLittleEndian_ulong(ref ns);
                    if (l0 == 8242000126441565528UL && l1 == 8391172886511248740UL)
                    {
                        flag = 1024UL;
                        values = ref _r.XForwardedHost;
                        nameStr = HeaderNames.XForwardedHost;
                    }
                    else if (l0 == 3275364211029339971UL && l1 == 7453010313431182917UL)
                    {
                        flag = 8388608UL;
                        values = ref _r.ContentEncoding;
                        nameStr = HeaderNames.ContentEncoding;
                    }
                    else if (l0 == 8017301761384477264UL && l1 == 7957695015191670382UL)
                    {
                        flag = 1099511627776UL;
                        values = ref _r.ProxyConnection;
                        nameStr = HeaderNames.ProxyConnection;
                    }
                    else if (l0 == 7810774964562505048UL && l1 == 4920514020217812065UL)
                    {
                        flag = 2199023255552UL;
                        values = ref _r.XCorrelationID;
                        nameStr = HeaderNames.XCorrelationID;
                    }
                }
                break;

            case 17:
                {
                    ref byte ns = ref nameStart;
                    var l0 = ReadUnalignedLittleEndian_ulong(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(8));
                    var l1 = ReadUnalignedLittleEndian_ulong(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(8));
                    if (l0 == 8242000126441565528UL && l1 == 8390050319499027812UL && ns == 111)
                    {
                        flag = 2048UL;
                        values = ref _r.XForwardedProto;
                        nameStr = HeaderNames.XForwardedProto;
                    }
                    else if (l0 == 7379539893622236745UL && l1 == 7164779863157794153UL && ns == 101)
                    {
                        flag = 134217728UL;
                        values = ref _r.IfModifiedSince;
                        nameStr = HeaderNames.IfModifiedSince;
                    }
                    else if (l0 == 8243107338930713172UL && l1 == 7956000646299010349UL && ns == 103)
                    {
                        flag = 549755813888UL;
                        values = ref _r.TransferEncoding;
                        nameStr = HeaderNames.TransferEncoding;
                    }
                }
                break;

            case 18:
                {
                    ref byte ns = ref nameStart;
                    var l0 = ReadUnalignedLittleEndian_ulong(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(8));
                    var l1 = ReadUnalignedLittleEndian_ulong(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(8));
                    var s2 = ReadUnalignedLittleEndian_ushort(ref ns);
                    if (l0 == 8449084375658623568UL && l1 == 7017568593162627188UL && s2 == 25972)
                    {
                        flag = 35184372088832UL;
                        values = ref _r.ProxyAuthenticate;
                        nameStr = HeaderNames.ProxyAuthenticate;
                    }
                }
                break;

            case 19:
                {
                    ref byte ns = ref nameStart;
                    var l0 = ReadUnalignedLittleEndian_ulong(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(8));
                    var l1 = ReadUnalignedLittleEndian_ulong(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(8));
                    var s2 = ReadUnalignedLittleEndian_ushort(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(2));
                    if (l0 == 7237123446850545225UL && l1 == 7589459706270803561UL && s2 == 25454 && ns == 101)
                    {
                        flag = 1073741824UL;
                        values = ref _r.IfUnmodifiedSince;
                        nameStr = HeaderNames.IfUnmodifiedSince;
                    }
                    else if (l0 == 8449084375658623568UL && l1 == 8386118574450632820UL && s2 == 28521 && ns == 110)
                    {
                        flag = 17179869184UL;
                        values = ref _r.ProxyAuthorization;
                        nameStr = HeaderNames.ProxyAuthorization;
                    }
                }
                break;

            case 25:
                {
                    ref byte ns = ref nameStart;
                    var l0 = ReadUnalignedLittleEndian_ulong(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(8));
                    var l1 = ReadUnalignedLittleEndian_ulong(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(8));
                    var l2 = ReadUnalignedLittleEndian_ulong(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(8));
                    if (l0 == 3271131074048520277UL && l1 == 7310034214940012105UL && l2 == 8391162085809410605UL && ns == 115)
                    {
                        flag = 32UL;
                        values = ref _r.UpgradeInsecureRequests;
                        nameStr = HeaderNames.UpgradeInsecureRequests;
                    }
                }
                break;

            case 29:
                {
                    ref byte ns = ref nameStart;
                    var l0 = ReadUnalignedLittleEndian_ulong(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(8));
                    var l1 = ReadUnalignedLittleEndian_ulong(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(8));
                    var l2 = ReadUnalignedLittleEndian_ulong(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(8));
                    var i3 = ReadUnalignedLittleEndian_uint(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(4));
                    if (l0 == 4840652113952596801UL && l1 == 5921508310979473007UL && l2 == 5561229153844687205UL && i3 == 1869116517U && ns == 100)
                    {
                        flag = 32768UL;
                        values = ref _r.AccessControlRequestMethod;
                        nameStr = HeaderNames.AccessControlRequestMethod;
                    }
                }
                break;

            case 30:
                {
                    ref byte ns = ref nameStart;
                    var l0 = ReadUnalignedLittleEndian_ulong(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(8));
                    var l1 = ReadUnalignedLittleEndian_ulong(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(8));
                    var l2 = ReadUnalignedLittleEndian_ulong(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(8));
                    var i3 = ReadUnalignedLittleEndian_uint(ref ns);
                    ns = ref Unsafe.AddByteOffset(ref ns, (IntPtr)(4));
                    var s4 = ReadUnalignedLittleEndian_ushort(ref ns);
                    if (l0 == 4840652113952596801UL && l1 == 5921508310979473007UL && l2 == 5200941183655047525UL && i3 == 1701077349U && s4 == 29554)
                    {
                        flag = 65536UL;
                        values = ref _r.AccessControlRequestHeaders;
                        nameStr = HeaderNames.AccessControlRequestHeaders;
                    }
                }
                break;

            default:
                break;
        }
        if (flag != 0UL)
        {
            var valueStr = value.GetRequestHeaderString(nameStr, checkForNewlineChars);
            if ((_bits & flag) == 0)
            {
                _bits |= flag;
                values = new StringValues(valueStr);
            }
            else
            {
                values = StringValues.Concat(values, valueStr);
            }
        }
        else
        {
            nameStr = name.GetHeaderName();
            var valueStr = value.GetRequestHeaderString(nameStr, checkForNewlineChars);
            dict.TryGetValue(nameStr, out var existing);
            dict[nameStr] = StringValues.Concat(existing, valueStr);
        }
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