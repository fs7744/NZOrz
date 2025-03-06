using Microsoft.Extensions.Primitives;

namespace NZ.Orz.Config;

public sealed record ProxyConfigSnapshot : IProxyConfig
{
    public IList<RouteConfig> Routes { get; set; }

    public IList<ClusterConfig> Clusters { get; set; }
}