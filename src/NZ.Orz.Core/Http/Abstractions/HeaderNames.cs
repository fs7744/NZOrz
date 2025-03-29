using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NZ.Orz.Http;

public static class HeaderNames
{
    public static readonly string AIM = "A-IM";
    public static readonly string Accept = "Accept";
    public static readonly string AcceptCharset = "Accept-Charset";
    public static readonly string AcceptDatetime = "Accept-Datetime";
    public static readonly string AcceptEncoding = "Accept-Encoding";
    public static readonly string AcceptLanguage = "Accept-Language";
    public static readonly string AccessControlRequestMethod = "Access-Control-Request-Method";
    public static readonly string AccessControlRequestHeaders = "Access-Control-Request-Headers";
    public static readonly string CacheControl = "Cache-Control";
    public static readonly string Connection = "Connection";
    public static readonly string ContentEncoding = "Content-Encoding";
    public static readonly string ContentLength = "Content-Length";
    public static readonly string ContentMD5 = "Content-MD5";
    public static readonly string ContentType = "Content-Type";
    public static readonly string Cookie = "Cookie";
    public static readonly string Expect = "Expect";
    public static readonly string Forwarded = "Forwarded";
    public static readonly string From = "From";
    public static readonly string Host = "Host";
    public static readonly string IfMatch = "If-Match";
    public static readonly string IfModifiedSince = "If-Modified-Since";
    public static readonly string IfNoneMatch = "If-None-Match";
    public static readonly string IfRange = "If-Range";
    public static readonly string IfUnmodifiedSince = "If-Unmodified-Since";
    public static readonly string MaxForwards = "Max-Forwards";
    public static readonly string Origin = "Origin";
    public static readonly string Pragma = "Pragma";
    public static readonly string Prefer = "Prefer";
    public static readonly string ProxyAuthorization = "Proxy-Authorization";
    public static readonly string Range = "Range";
    public static readonly string Referer = "Referer";
    public static readonly string TE = "TE";
    public static readonly string Trailer = "Trailer";
    public static readonly string TransferEncoding = "Transfer-Encoding";
    public static readonly string UserAgent = "User-Agent";
    public static readonly string Upgrade = "Upgrade";
    public static readonly string Via = "Via";
    public static readonly string Warning = "Warning";

    public static readonly string UpgradeInsecureRequests = "Upgrade-Insecure-Requests";
    public static readonly string XRequestedWith = "X-Requested-With";
    public static readonly string DNT = "DNT";
    public static readonly string XForwardedFor = "X-Forwarded-For";
    public static readonly string XForwardedHost = "X-Forwarded-Host";
    public static readonly string XForwardedProto = "X-Forwarded-Proto";
    public static readonly string FrontEndHttps = "Front-End-Https";
    public static readonly string XHttpMethodOverride = "X-Http-Method-Override";
    public static readonly string XCsrfToken = "X-Csrf-Token";
    public static readonly string XRequestID = "X-Request-ID";
    public static readonly string ProxyConnection = "Proxy-Connection";
    public static readonly string XCorrelationID = "X-Correlation-ID";
    public static readonly string CorrelationID = "Correlation-ID";
    public static readonly string RequestId = "Request-Id";
    public static readonly string KeepAlive = "Keep-Alive";
    public static readonly string ProxyAuthenticate = "Proxy-Authenticate";
    public static readonly string TraceParent = "traceparent";
    public static readonly string TraceState = "tracestate";

    /// <summary>Gets the <c>Date</c> HTTP header name.</summary>
    public static readonly string Date = "Date";

    /// <summary>Gets the <c>Accept-Ranges</c> HTTP header name.</summary>
    public static readonly string AcceptRanges = "Accept-Ranges";

    /// <summary>Gets the <c>Access-Control-Allow-Credentials</c> HTTP header name.</summary>
    public static readonly string AccessControlAllowCredentials = "Access-Control-Allow-Credentials";

    /// <summary>Gets the <c>Access-Control-Allow-Headers</c> HTTP header name.</summary>
    public static readonly string AccessControlAllowHeaders = "Access-Control-Allow-Headers";

    /// <summary>Gets the <c>Access-Control-Allow-Methods</c> HTTP header name.</summary>
    public static readonly string AccessControlAllowMethods = "Access-Control-Allow-Methods";

    /// <summary>Gets the <c>Access-Control-Allow-Origin</c> HTTP header name.</summary>
    public static readonly string AccessControlAllowOrigin = "Access-Control-Allow-Origin";

    /// <summary>Gets the <c>Access-Control-Expose-Headers</c> HTTP header name.</summary>
    public static readonly string AccessControlExposeHeaders = "Access-Control-Expose-Headers";

    /// <summary>Gets the <c>Access-Control-Max-Age</c> HTTP header name.</summary>
    public static readonly string AccessControlMaxAge = "Access-Control-Max-Age";

    /// <summary>Gets the <c>Age</c> HTTP header name.</summary>
    public static readonly string Age = "Age";

    /// <summary>Gets the <c>Allow</c> HTTP header name.</summary>
    public static readonly string Allow = "Allow";

    /// <summary>Gets the <c>Alt-Svc</c> HTTP header name.</summary>
    public static readonly string AltSvc = "Alt-Svc";

    /// <summary>Gets the <c>Authorization</c> HTTP header name.</summary>
    public static readonly string Authorization = "Authorization";

    /// <summary>Gets the <c>baggage</c> HTTP header name.</summary>
    public static readonly string Baggage = "baggage";

    /// <summary>Gets the <c>Content-Disposition</c> HTTP header name.</summary>
    public static readonly string ContentDisposition = "Content-Disposition";

    /// <summary>Gets the <c>Content-Language</c> HTTP header name.</summary>
    public static readonly string ContentLanguage = "Content-Language";

    /// <summary>Gets the <c>Content-Location</c> HTTP header name.</summary>
    public static readonly string ContentLocation = "Content-Location";

    /// <summary>Gets the <c>Content-Range</c> HTTP header name.</summary>
    public static readonly string ContentRange = "Content-Range";

    /// <summary>Gets the <c>Content-Security-Policy</c> HTTP header name.</summary>
    public static readonly string ContentSecurityPolicy = "Content-Security-Policy";

    /// <summary>Gets the <c>Content-Security-Policy-Report-Only</c> HTTP header name.</summary>
    public static readonly string ContentSecurityPolicyReportOnly = "Content-Security-Policy-Report-Only";

    /// <summary>Gets the <c>Correlation-Context</c> HTTP header name.</summary>
    public static readonly string CorrelationContext = "Correlation-Context";

    /// <summary>Gets the <c>ETag</c> HTTP header name.</summary>
    public static readonly string ETag = "ETag";

    /// <summary>Gets the <c>Expires</c> HTTP header name.</summary>
    public static readonly string Expires = "Expires";

    /// <summary>Gets the <c>Grpc-Accept-Encoding</c> HTTP header name.</summary>
    public static readonly string GrpcAcceptEncoding = "Grpc-Accept-Encoding";

    /// <summary>Gets the <c>Grpc-Encoding</c> HTTP header name.</summary>
    public static readonly string GrpcEncoding = "Grpc-Encoding";

    /// <summary>Gets the <c>Grpc-Message</c> HTTP header name.</summary>
    public static readonly string GrpcMessage = "Grpc-Message";

    /// <summary>Gets the <c>Grpc-Status</c> HTTP header name.</summary>
    public static readonly string GrpcStatus = "Grpc-Status";

    /// <summary>Gets the <c>Grpc-Timeout</c> HTTP header name.</summary>
    public static readonly string GrpcTimeout = "Grpc-Timeout";

    /// <summary>Gets the <c>Last-Modified</c> HTTP header name.</summary>
    public static readonly string LastModified = "Last-Modified";

    /// <summary>Gets the <c>Link</c> HTTP header name.</summary>
    public static readonly string Link = "Link";

    /// <summary>Gets the <c>Location</c> HTTP header name.</summary>
    public static readonly string Location = "Location";

    /// <summary>Gets the <c>Retry-After</c> HTTP header name.</summary>
    public static readonly string RetryAfter = "Retry-After";

    /// <summary>Gets the <c>Sec-WebSocket-Accept</c> HTTP header name.</summary>
    public static readonly string SecWebSocketAccept = "Sec-WebSocket-Accept";

    /// <summary>Gets the <c>Sec-WebSocket-Key</c> HTTP header name.</summary>
    public static readonly string SecWebSocketKey = "Sec-WebSocket-Key";

    /// <summary>Gets the <c>Sec-WebSocket-Protocol</c> HTTP header name.</summary>
    public static readonly string SecWebSocketProtocol = "Sec-WebSocket-Protocol";

    /// <summary>Gets the <c>Sec-WebSocket-Version</c> HTTP header name.</summary>
    public static readonly string SecWebSocketVersion = "Sec-WebSocket-Version";

    /// <summary>Gets the <c>Sec-WebSocket-Extensions</c> HTTP header name.</summary>
    public static readonly string SecWebSocketExtensions = "Sec-WebSocket-Extensions";

    /// <summary>Gets the <c>Server</c> HTTP header name.</summary>
    public static readonly string Server = "Server";

    /// <summary>Gets the <c>Set-Cookie</c> HTTP header name.</summary>
    public static readonly string SetCookie = "Set-Cookie";

    /// <summary>Gets the <c>Strict-Transport-Security</c> HTTP header name.</summary>
    public static readonly string StrictTransportSecurity = "Strict-Transport-Security";

    /// <summary>Gets the <c>Translate</c> HTTP header name.</summary>
    public static readonly string Translate = "Translate";

    /// <summary>Gets the <c>Vary</c> HTTP header name.</summary>
    public static readonly string Vary = "Vary";

    /// <summary>Gets the <c>WWW-Authenticate</c> HTTP header name.</summary>
    public static readonly string WWWAuthenticate = "WWW-Authenticate";

    /// <summary>Gets the <c>X-Content-Type-Options</c> HTTP header name.</summary>
    public static readonly string XContentTypeOptions = "X-Content-Type-Options";

    /// <summary>Gets the <c>X-Frame-Options</c> HTTP header name.</summary>
    public static readonly string XFrameOptions = "X-Frame-Options";

    /// <summary>Gets the <c>X-Powered-By</c> HTTP header name.</summary>
    public static readonly string XPoweredBy = "X-Powered-By";

    /// <summary>Gets the <c>X-UA-Compatible</c> HTTP header name.</summary>
    public static readonly string XUACompatible = "X-UA-Compatible";

    /// <summary>Gets the <c>X-XSS-Protection</c> HTTP header name.</summary>
    public static readonly string XXSSProtection = "X-XSS-Protection";

    private static readonly FrozenSet<string> _internedHeaderNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            HeaderNames.Accept,
            HeaderNames.AcceptCharset,
            HeaderNames.AcceptEncoding,
            HeaderNames.AcceptLanguage,
            HeaderNames.AcceptRanges,
            HeaderNames.AccessControlAllowCredentials,
            HeaderNames.AccessControlAllowHeaders,
            HeaderNames.AccessControlAllowMethods,
            HeaderNames.AccessControlAllowOrigin,
            HeaderNames.AccessControlExposeHeaders,
            HeaderNames.AccessControlMaxAge,
            HeaderNames.AccessControlRequestHeaders,
            HeaderNames.AccessControlRequestMethod,
            HeaderNames.Age,
            HeaderNames.Allow,
            HeaderNames.AltSvc,
            HeaderNames.Authorization,
            HeaderNames.Baggage,
            HeaderNames.CacheControl,
            HeaderNames.Connection,
            HeaderNames.ContentDisposition,
            HeaderNames.ContentEncoding,
            HeaderNames.ContentLanguage,
            HeaderNames.ContentLength,
            HeaderNames.ContentLocation,
            HeaderNames.ContentMD5,
            HeaderNames.ContentRange,
            HeaderNames.ContentSecurityPolicy,
            HeaderNames.ContentSecurityPolicyReportOnly,
            HeaderNames.ContentType,
            HeaderNames.CorrelationContext,
            HeaderNames.Cookie,
            HeaderNames.Date,
            HeaderNames.DNT,
            HeaderNames.ETag,
            HeaderNames.Expires,
            HeaderNames.Expect,
            HeaderNames.From,
            HeaderNames.GrpcAcceptEncoding,
            HeaderNames.GrpcEncoding,
            HeaderNames.GrpcMessage,
            HeaderNames.GrpcStatus,
            HeaderNames.GrpcTimeout,
            HeaderNames.Host,
            HeaderNames.KeepAlive,
            HeaderNames.IfMatch,
            HeaderNames.IfModifiedSince,
            HeaderNames.IfNoneMatch,
            HeaderNames.IfRange,
            HeaderNames.IfUnmodifiedSince,
            HeaderNames.LastModified,
            HeaderNames.Link,
            HeaderNames.Location,
            HeaderNames.MaxForwards,
            HeaderNames.Origin,
            HeaderNames.Pragma,
            HeaderNames.ProxyAuthenticate,
            HeaderNames.ProxyAuthorization,
            HeaderNames.ProxyConnection,
            HeaderNames.Range,
            HeaderNames.Referer,
            HeaderNames.RetryAfter,
            HeaderNames.RequestId,
            HeaderNames.SecWebSocketAccept,
            HeaderNames.SecWebSocketKey,
            HeaderNames.SecWebSocketProtocol,
            HeaderNames.SecWebSocketVersion,
            HeaderNames.SecWebSocketExtensions,
            HeaderNames.Server,
            HeaderNames.SetCookie,
            HeaderNames.StrictTransportSecurity,
            HeaderNames.TE,
            HeaderNames.Trailer,
            HeaderNames.TransferEncoding,
            HeaderNames.Translate,
            HeaderNames.TraceParent,
            HeaderNames.TraceState,
            HeaderNames.Upgrade,
            HeaderNames.UpgradeInsecureRequests,
            HeaderNames.UserAgent,
            HeaderNames.Vary,
            HeaderNames.Via,
            HeaderNames.Warning,
            HeaderNames.WWWAuthenticate,
            HeaderNames.XContentTypeOptions,
            HeaderNames.XFrameOptions,
            HeaderNames.XPoweredBy,
            HeaderNames.XRequestedWith,
            HeaderNames.XUACompatible,
            HeaderNames.XXSSProtection,
        }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetInternedHeaderName(string name)
    {
        // Some headers can be very long lived; for example those on a WebSocket connection
        // so we exchange these for the preallocated strings predefined in HeaderNames
        if (_internedHeaderNames.TryGetValue(name, out var internedName))
        {
            return internedName;
        }

        return name;
    }
}