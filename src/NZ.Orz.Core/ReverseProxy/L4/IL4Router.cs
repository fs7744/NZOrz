using NZ.Orz.Config;
using NZ.Orz.Connections;
using System.IO.Pipelines;

namespace NZ.Orz.ReverseProxy.L4;

public interface IL4Router
{
    ValueTask<RouteConfig> MatchAsync(ConnectionContext context);

    ValueTask<(RouteConfig, byte[])> MatchSNIAsync(ConnectionContext context, CancellationToken token);

    Task ReBulidAsync(IProxyConfig proxyConfig, ServerOptions serverOptions);
}