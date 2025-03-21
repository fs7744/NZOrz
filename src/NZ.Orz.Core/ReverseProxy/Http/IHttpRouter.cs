using NZ.Orz.Config;

namespace NZ.Orz.ReverseProxy.Http;

public interface IHttpRouter
{
    Task ReBulidAsync(IProxyConfig proxyConfig, ServerOptions options);
}