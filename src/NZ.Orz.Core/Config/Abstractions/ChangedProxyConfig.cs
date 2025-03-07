namespace NZ.Orz.Config;

public class ChangedProxyConfig
{
    public List<ListenOptions> EndpointsToStop { get; set; }

    public List<ListenOptions> EndpointsToStart { get; set; }

    public bool RouteChanged { get; set; }
    public IProxyConfig ProxyConfig { get; set; }
    public IEnumerable<ClusterConfig> NewClusters { get; set; }
}