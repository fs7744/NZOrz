using NZ.Orz.Sockets;

namespace NZ.Orz.Config.Memory;

public class MemoryReverseProxyConfigBuilder
{
    public ServerOptions ServerOptions { get; internal set; } = new ServerOptions();

    private SocketTransportOptions _SocketTransportOptions;

    public SocketTransportOptions SocketTransportOptions

    {
        get
        {
            if (_SocketTransportOptions == null)
                _SocketTransportOptions = new SocketTransportOptions();
            return _SocketTransportOptions;
        }
    }

    public List<MemoryRouteConfig> Routes { get; internal set; } = new List<MemoryRouteConfig>();

    public List<MemoryClusterConfig> Clusters { get; internal set; } = new List<MemoryClusterConfig>();

    internal IRouteContractor Build()
    {
        return new MemoryRouteContractor(this);
    }
}