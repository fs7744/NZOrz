namespace NZ.Orz.Config;

public class ChangedProxyConfig
{
    public IEnumerable<ListenOptions> EndpointsToStop { get; set; }

    public IEnumerable<ListenOptions> EndpointsToStart { get; set; }

    public bool L4Changed { get; set; }
    public IProxyConfig ProxyConfig { get; set; }
}