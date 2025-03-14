using DotNext;
using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Routing;

namespace NZ.Orz.ReverseProxy.L4;

public class L4Router : IL4Router
{
    private RouteTable<RouteConfig> routeTable;
    private RouteTable<RouteConfig> sniRoute;

    public ValueTask<RouteConfig> MatchAsync(ConnectionContext context)
    {
        if (routeTable is null) return ValueTask.FromResult<RouteConfig>(null);
        return routeTable.MatchAsync(context.LocalEndPoint.ToString().Reverse(), context.Protocols, Match);
    }

    private static bool Match(RouteConfig config, GatewayProtocols protocols)
    {
        return config.Protocols.HasFlag(protocols);
    }

    public async Task ReBulidAsync(IProxyConfig proxyConfig, ServerOptions serverOptions)
    {
        var old = routeTable;
        var oldSniRoute = sniRoute;
        (routeTable, sniRoute) = BuildL4RouteTable(proxyConfig, serverOptions);
        if (old != null)
            await old.DisposeAsync();
        if (oldSniRoute != null)
            await oldSniRoute.DisposeAsync();
    }

    private (RouteTable<RouteConfig> l4, RouteTable<RouteConfig> sni) BuildL4RouteTable(IProxyConfig config, ServerOptions serverOptions)
    {
        var builder = new RouteTableBuilder<RouteConfig>(serverOptions.RouteComparison, serverOptions.RouteCahceSize);
        var sniRouteBuilder = new RouteTableBuilder<RouteConfig>(serverOptions.RouteComparison, serverOptions.RouteCahceSize);
        var hasL4 = false;
        var hasSni = false;
        foreach (var route in config.Routes.Where(i => i.Protocols.HasFlag(GatewayProtocols.TCP) || i.Protocols.HasFlag(GatewayProtocols.UDP) || i.Protocols.HasFlag(GatewayProtocols.SNI)))
        {
            RouteTableBuilder<RouteConfig> b;
            if (route.Protocols.HasFlag(GatewayProtocols.SNI))
            {
                b = sniRouteBuilder;
                hasSni = true;
            }
            else
            {
                hasL4 = true;
                b = builder;
            };
            foreach (var host in route.Match.Hosts)
            {
                if (host.StartsWith("localhost:"))
                {
                    Set(b, route, $"127.0.0.1:{host.AsSpan(10)}");
                    Set(b, route, $"[::1]:{host.AsSpan(10)}");
                }
                Set(b, route, host);
            }
        }
        return (hasL4 ? builder.Build() : null, hasSni ? sniRouteBuilder.Build() : null);

        static void Set(RouteTableBuilder<RouteConfig> builder, RouteConfig? route, string host)
        {
            if (host.StartsWith('*'))
            {
                builder.Add(host[1..].Reverse(), route, RouteType.Prefix, route.Order);
            }
            else
            {
                builder.Add(host.Reverse(), route, RouteType.Exact, route.Order);
            }
        }
    }

    public ValueTask<RouteConfig> MatchSNIAsync(ConnectionContext context)
    {
        throw new NotImplementedException();
    }
}