using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NZ.Orz.Config;
using NZ.Orz.Servers;

namespace NZ.Orz.Hosting;

internal class HostedService : IHostedService, IAsyncDisposable
{
    private readonly IRouteContractor contractor;
    private readonly IServiceProvider serviceProvider;
    private IServer server;

    public HostedService(IRouteContractor contractor, IServiceProvider serviceProvider)
    {
        this.contractor = contractor;
        this.serviceProvider = serviceProvider;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync(new CancellationToken(canceled: true)).ConfigureAwait(false);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await contractor.LoadAsync(cancellationToken);
        server = serviceProvider.GetRequiredService<IServer>();
        await server.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await contractor.StopAsync(cancellationToken);
        await server.StopAsync(cancellationToken);
    }
}