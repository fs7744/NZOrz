using Microsoft.Extensions.Primitives;

namespace NZ.Orz.Config;

public sealed record ProxyConfigSnapshot : IProxyConfig
{
    public IReadOnlyList<RouteConfig> Routes { get; set; }

    public IReadOnlyList<ClusterConfig> Clusters { get; set; }
}