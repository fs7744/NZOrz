using NZ.Orz.Config;
using NZ.Orz.Connections;

namespace NZ.Orz.ReverseProxy.L4;

public interface IL4Router
{
    ValueTask<RouteConfig> MatchAsync(ConnectionContext context);

    Task ReBulidAsync(IProxyConfig proxyConfig, ServerOptions serverOptions);
}