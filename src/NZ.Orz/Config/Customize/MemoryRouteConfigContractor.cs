using Microsoft.Extensions.Primitives;

namespace NZ.Orz.Config.Customize;

internal class MemoryRouteConfigContractor : IRouteContractor
{
    private readonly ServerOptions serverOptions;
    private ListenOptions[] listenOptions;

    public MemoryRouteConfigContractor(ServerOptions serverOptions, ListenOptions[] listenOptions)
    {
        this.serverOptions = serverOptions;
        this.listenOptions = listenOptions;
    }

    public IEnumerable<ListenOptions> GetListenOptions()
    {
        return listenOptions;
    }

    public IChangeToken? GetReloadToken()
    {
        return null;
    }

    public ServerOptions GetServerOptions()
    {
        return serverOptions;
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