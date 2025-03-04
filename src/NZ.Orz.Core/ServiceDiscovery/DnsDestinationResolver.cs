using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NZ.Orz.Config;
using NZ.Orz.Health;
using NZ.Orz.Infrastructure;
using System.Net;

namespace NZ.Orz.ServiceDiscovery;

public class DnsDestinationResolver : DestinationResolverBase
{
    private readonly ILogger<DnsDestinationResolver> logger;
    private readonly IHealthUpdater healthUpdater;
    private ServerOptions options;

    public DnsDestinationResolver(IRouteContractor contractor, ILogger<DnsDestinationResolver> logger, IHealthUpdater healthUpdater)
    {
        options = contractor.GetServerOptions();
        this.logger = logger;
        this.healthUpdater = healthUpdater;
    }

    public override int Order => 0;

    public override async Task ResolveAsync(FuncDestinationResolverState state, CancellationToken cancellationToken)
    {
        List<DestinationState> destinations = new List<DestinationState>();
        foreach (var item in state.Configs)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (hostName, port) = AddressParser.Parse(item.Address);
            try
            {
                var addresses = options.DnsAddressFamily switch
                {
                    { } addressFamily => await Dns.GetHostAddressesAsync(hostName, addressFamily, cancellationToken).ConfigureAwait(false),
                    null => await Dns.GetHostAddressesAsync(hostName, cancellationToken).ConfigureAwait(false)
                };
                destinations.AddRange(addresses.Select(i => new DestinationState() { EndPoint = new IPEndPoint(i, port), ClusterConfig = state.Cluster }));
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Failed to resolve host '{hostName}'. See {nameof(Exception.InnerException)} for details.", exception);
            }
        }

        if (options.DnsRefreshPeriod.HasValue && options.DnsRefreshPeriod > TimeSpan.Zero)
        {
            var cts = state.CancellationTokenSource;
            if (cts != null && !cts.IsCancellationRequested)
            {
                cts.Cancel();
            }
            state.CancellationTokenSource = cts = new CancellationTokenSource(options.DnsRefreshPeriod.Value);
            new CancellationChangeToken(cts.Token).RegisterChangeCallback(o =>
            {
                if (o is FuncDestinationResolverState s)
                {
                    try
                    {
                        ResolveAsync(s, new CancellationTokenSource(options.DnsRefreshPeriod.Value).Token).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, ex.Message);
                    }
                }
            }, this);
        }

        state.Destinations = destinations;
        healthUpdater.UpdateAvailableDestinations(state.Cluster);
    }
}