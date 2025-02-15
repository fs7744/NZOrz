using Microsoft.Extensions.Primitives;

namespace NZ.Orz.Config;

public interface IRouteContractor
{
    IEnumerable<ListenOptions> GetListenOptions();

    IChangeToken? GetReloadToken();

    ServerOptions GetServerOptions();

    Task LoadAsync(CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);
}