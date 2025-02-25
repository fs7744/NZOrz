using Microsoft.Extensions.Primitives;
using NZ.Orz.Sockets;

namespace NZ.Orz.Config.Memory;

public sealed class MemoryRouteContractor : IRouteContractor
{
    private readonly ProxyConfigSnapshot proxyConfig;
    private readonly ServerOptions serverOptions;
    private readonly SocketTransportOptions socketTransportOptions;

    public MemoryRouteContractor(MemoryReverseProxyConfigBuilder builder)
    {
        proxyConfig = new ProxyConfigSnapshot()
        {
            Clusters = builder.Clusters.Select(i => i.Build()).ToList(),
            Routes = builder.Routes.Select(i => i.Build()).ToList(),
        };
        serverOptions = builder.ServerOptions;
        socketTransportOptions = builder.SocketTransportOptions;
    }

    public IEnumerable<ListenOptions> GetListenOptions()
    {
        throw new NotImplementedException();
    }

    public IProxyConfig GetProxyConfig()
    {
        return proxyConfig;
    }

    public IChangeToken? GetReloadToken()
    {
        return null;
    }

    public ServerOptions GetServerOptions()
    {
        return serverOptions;
    }

    public SocketTransportOptions? GetSocketTransportOptions()
    {
        return socketTransportOptions;
    }

    public Task LoadAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}