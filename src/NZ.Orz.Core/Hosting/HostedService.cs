using Microsoft.Extensions.Hosting;
using NZ.Orz.Config;
using NZ.Orz.Servers;

namespace NZ.Orz.Hosting;

internal class HostedService : IHostedService, IAsyncDisposable
{
    private readonly IRouteContractor contractor;
    private readonly IServer server;

    public HostedService(IRouteContractor contractor, IServer server)
    {
        this.contractor = contractor;
        this.server = server;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync(new CancellationToken(canceled: true)).ConfigureAwait(false);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await contractor.LoadAsync(cancellationToken);
        await server.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await contractor.StopAsync(cancellationToken);
        await server.StopAsync(cancellationToken);
    }
}