using NZ.Orz.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

[Flags]
public enum test
{
    Unknown = 0,
    Accept = 1,
    AcceptCharset = 1 << 1,
    AcceptEncoding = 1 << 2,
    AcceptLanguage = 1 << 3,
    AcceptRanges = 1 << 4,
    AccessControlAllowCredentials = 1 << 5,
    AccessControlAllowHeaders = 1 << 6,
}

public static class Test
{
    public static void Run()
    {
        var headers = new List<string>()
        {
            HeaderNames.Host,
HeaderNames.Connection,
HeaderNames.ContentLength,
HeaderNames.UserAgent,
HeaderNames.Upgrade,
HeaderNames.UpgradeInsecureRequests,
HeaderNames.Cookie,
HeaderNames.TraceParent,
HeaderNames.TraceState,
HeaderNames.XForwardedFor,
HeaderNames.XForwardedHost,
HeaderNames.XForwardedProto,
HeaderNames.Origin,
HeaderNames.CacheControl,
HeaderNames.ContentType,
HeaderNames.AccessControlRequestMethod,
HeaderNames.AccessControlRequestHeaders,
HeaderNames.XRequestID,
HeaderNames.Accept,
HeaderNames.AcceptCharset,
HeaderNames.AcceptDatetime,
HeaderNames.AcceptEncoding,
HeaderNames.AcceptLanguage,
HeaderNames.ContentEncoding,
HeaderNames.ContentMD5,
HeaderNames.Expect,
HeaderNames.IfMatch,
HeaderNames.IfModifiedSince,
HeaderNames.IfNoneMatch,
HeaderNames.IfRange,
HeaderNames.IfUnmodifiedSince,
HeaderNames.MaxForwards,
HeaderNames.Pragma,
HeaderNames.Prefer,
HeaderNames.ProxyAuthorization,
HeaderNames.Range,
HeaderNames.Referer,
HeaderNames.TE,
HeaderNames.Trailer,
HeaderNames.TransferEncoding,
HeaderNames.ProxyConnection,
HeaderNames.XCorrelationID,
HeaderNames.CorrelationID,
HeaderNames.RequestId,
HeaderNames.KeepAlive,
HeaderNames.ProxyAuthenticate,
HeaderNames.Forwarded,
            HeaderNames.XCsrfToken,
        };

        var a = headers.Select(i => Encoding.ASCII.GetBytes(i)).ToArray();
        ref byte nameStart = ref MemoryMarshal.GetReference(a[0].AsSpan());
        var h = HttpRequestHeaders.ReadUnalignedLittleEndian_uint(ref nameStart);
        var h2 = 1953722184U;
        if (h2 == h)
        {
            HeaderNames.Host.ToString();

            var aa = sizeof(ushort);
        }
    }
}