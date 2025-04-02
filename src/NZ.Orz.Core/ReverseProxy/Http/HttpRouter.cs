using NZ.Orz.Config;

namespace NZ.Orz.ReverseProxy.Http;

public class HttpRouter : IHttpRouter
{
    public Task ReBulidAsync(IProxyConfig proxyConfig, ServerOptions options)
    {
        // todo
        return Task.CompletedTask;
    }
}