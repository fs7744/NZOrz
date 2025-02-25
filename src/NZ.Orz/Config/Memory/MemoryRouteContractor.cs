using Microsoft.Extensions.Primitives;
using NZ.Orz.Sockets;

namespace NZ.Orz.Config.Memory;

public sealed class MemoryRouteContractor : IRouteContractor
{
    public MemoryRouteContractor(MemoryReverseProxyConfigBuilder memoryReverseProxyConfigBuilder)
    {
    }

    public IEnumerable<ListenOptions> GetListenOptions()
    {
        throw new NotImplementedException();
    }

    public IProxyConfig GetProxyConfig()
    {
        throw new NotImplementedException();
    }

    public IChangeToken? GetReloadToken()
    {
        throw new NotImplementedException();
    }

    public ServerOptions GetServerOptions()
    {
        throw new NotImplementedException();
    }

    public SocketTransportOptions? GetSocketTransportOptions()
    {
        throw new NotImplementedException();
    }

    public Task LoadAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}