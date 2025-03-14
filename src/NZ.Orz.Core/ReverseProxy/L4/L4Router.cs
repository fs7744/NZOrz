using DotNext;
using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Routing;
using NZ.Orz.Sockets;

namespace NZ.Orz.ReverseProxy.L4;

public class L4Router : IL4Router
{
    private RouteTable<RouteConfig> routeTable;

    public ValueTask<RouteConfig> MatchAsync(ConnectionContext context)
    {
        if (routeTable is null) return ValueTask.FromResult<RouteConfig>(null);
        return routeTable.MatchAsync(context.LocalEndPoint.ToString().Reverse(), context is UdpConnectionContext ? GatewayProtocols.UDP : GatewayProtocols.TCP, Match);
    }

    private static bool Match(RouteConfig config, GatewayProtocols protocols)
    {
        return config.Protocols.HasFlag(protocols);
    }

    public async Task ReBulidAsync(IProxyConfig proxyConfig, ServerOptions serverOptions)
    {
        var old = routeTable;
        routeTable = BuildL4RouteTable(proxyConfig, serverOptions);
        if (old != null)
            await old.DisposeAsync();
    }

    private RouteTable<RouteConfig> BuildL4RouteTable(IProxyConfig config, ServerOptions serverOptions)
    {
        var builder = new RouteTableBuilder<RouteConfig>(serverOptions.RouteComparison, serverOptions.RouteCahceSize);
        foreach (var route in config.Routes.Where(i => i.Protocols.HasFlag(GatewayProtocols.TCP) || i.Protocols.HasFlag(GatewayProtocols.UDP)))
        {
            foreach (var host in route.Match.Hosts)
            {
                if (host.StartsWith("localhost:"))
                {
                    Set(builder, route, $"127.0.0.1:{host.AsSpan(10)}");
                    Set(builder, route, $"[::1]:{host.AsSpan(10)}");
                }
                Set(builder, route, host);
            }
        }
        return builder.Build();

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
}