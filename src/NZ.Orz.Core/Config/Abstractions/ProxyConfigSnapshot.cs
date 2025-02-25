using Microsoft.Extensions.Primitives;

namespace NZ.Orz.Config;

public sealed record ProxyConfigSnapshot : IProxyConfig
{
    public IReadOnlyList<RouteConfig> Routes { get; init; }

    public IReadOnlyList<ClusterConfig> Clusters { get; init; }

    public IChangeToken ChangeToken => null;
}