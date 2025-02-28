using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Routing;

namespace NZ.Orz.ReverseProxy.L4;

public class L4Router : IL4Router
{
    public RouteTable<RouteConfig> RouteTable { get; set; }

    public ValueTask<RouteConfig> MatchAsync(ConnectionContext context)
    {
        return RouteTable == null ? ValueTask.FromResult<RouteConfig>(null) : RouteTable.FirstAsync(context.LocalEndPoint.ToString());
    }
}