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