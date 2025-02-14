namespace NZ.Orz.Config.Customize;

internal class MemoryRouteConfigContractor : IRouteContractor
{
    private ListenOptions[] listenOptions;

    public MemoryRouteConfigContractor(ListenOptions[] listenOptions)
    {
        this.listenOptions = listenOptions;
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