namespace NZ.Orz.Config;

public sealed record ProxyConfigSnapshot : IProxyConfig
{
    public IReadOnlyList<RouteConfig> Routes { get; set; }

    public IReadOnlyDictionary<string, ClusterConfig> Clusters { get; set; }
}