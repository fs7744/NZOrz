using Microsoft.Extensions.Logging;
using NZ.Orz.Config;

namespace NZ.Orz.ServiceDiscovery;

public class DnsDestinationResolver : IDestinationResolver
{
    private readonly IRouteContractor contractor;
    private readonly ILogger<DnsDestinationResolver> logger;

    public DnsDestinationResolver(IRouteContractor contractor, ILogger<DnsDestinationResolver> logger)
    {
        this.contractor = contractor;
        this.logger = logger;
    }

    public async Task<IDestinationResolverState> ResolveDestinationsAsync(List<DestinationConfig> destinationConfigs, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var options = contractor.GetServerOptions();
        var r = new DnsDestinationResolverState(options, destinationConfigs, logger);
        await r.LoadAsync(cancellationToken);
        return r;
    }
}