using System.Diagnostics.CodeAnalysis;

namespace NZ.Orz.Config.Customize;

internal class MemoryRouteConfigContractor : IRouteContractor
{
    private Action<RouteConfigBuilder> action;
    private readonly IServiceProvider provider;

    public MemoryRouteConfigContractor(Action<RouteConfigBuilder> action, IServiceProvider provider)
    {
        this.action = action;
        this.provider = provider;
    }

    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        var c = action;
        var builder = new RouteConfigBuilder();
        builder.ServiceProvider = provider;
        action(builder);
        action = null;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}