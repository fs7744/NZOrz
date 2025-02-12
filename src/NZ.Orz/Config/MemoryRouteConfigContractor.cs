using System.Diagnostics.CodeAnalysis;

namespace NZ.Orz.Config;

internal class MemoryRouteConfigContractor : IRouteContractor
{
    private RouteConfig config;

    public MemoryRouteConfigContractor([NotNull] RouteConfig config)
    {
        this.config = config;
    }

    public Task<RouteConfig> LoadAllAsync(CancellationToken cancellationToken)
    {
        var c = config;
        var r = Task.FromResult(c);
        config = null;
        return r;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}