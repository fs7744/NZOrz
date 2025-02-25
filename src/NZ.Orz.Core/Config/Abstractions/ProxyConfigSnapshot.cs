using Microsoft.Extensions.Primitives;

namespace NZ.Orz.Config;

public sealed class ProxyConfigSnapshot : IProxyConfig
{
    public IReadOnlyList<RouteConfig> Routes { get; internal set; } = new List<RouteConfig>();

    public IReadOnlyList<ClusterConfig> Clusters { get; internal set; } = new List<ClusterConfig>();

    public IChangeToken ChangeToken => null;
}