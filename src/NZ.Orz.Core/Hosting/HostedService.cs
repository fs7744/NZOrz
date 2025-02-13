using Microsoft.Extensions.Hosting;
using NZ.Orz.Config;

namespace NZ.Orz.Hosting;

internal class HostedService : IHostedService, IAsyncDisposable
{
    private readonly IRouteContractor contractor;

    public HostedService(IRouteContractor contractor)
    {
        this.contractor = contractor;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync(new CancellationToken(canceled: true)).ConfigureAwait(false);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await contractor.LoadAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await contractor.StopAsync(cancellationToken);
    }
}