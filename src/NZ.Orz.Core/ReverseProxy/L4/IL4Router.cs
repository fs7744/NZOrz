using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Routing;

namespace NZ.Orz.ReverseProxy.L4;

public interface IL4Router
{
    RouteTable<RouteConfig> RouteTable { get; set; }

    ValueTask<RouteConfig> MatchAsync(ConnectionContext context);
}