using Microsoft.Extensions.Primitives;
using NZ.Orz.Sockets;

namespace NZ.Orz.Config;

public interface IRouteContractor
{
    IEnumerable<ListenOptions> GetListenOptions();

    IChangeToken? GetReloadToken();

    ServerOptions GetServerOptions();

    SocketTransportOptions? GetSocketTransportOptions();

    IProxyConfig GetProxyConfig();

    Task LoadAsync(CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);

    Task<ChangedProxyConfig> ReloadAsync();
}