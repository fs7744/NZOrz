using DotNext;
using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Routing;
using NZ.Orz.Sockets;

namespace NZ.Orz.ReverseProxy.L4;

public class L4Router : IL4Router
{
    public RouteTable<RouteConfig> RouteTable { get; set; }

    public ValueTask<RouteConfig> MatchAsync(ConnectionContext context)
    {
        if (RouteTable is null) return ValueTask.FromResult<RouteConfig>(null);
        return RouteTable.MatchAsync(context.LocalEndPoint.ToString().Reverse(), context is UdpConnectionContext ? GatewayProtocols.UDP : GatewayProtocols.TCP, Match);
    }

    private static bool Match(RouteConfig config, GatewayProtocols protocols)
    {
        return config.Protocols.HasFlag(protocols);
    }
}