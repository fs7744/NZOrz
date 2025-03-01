using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NZ.Orz.Config;
using NZ.Orz.Infrastructure;
using System.Collections;
using System.Net;

namespace NZ.Orz.ServiceDiscovery;

public class DnsDestinationResolverState : IDestinationResolverState
{
    private ServerOptions options;
    private List<DestinationConfig> destinationConfigs;
    private readonly ILogger logger;
    private List<DestinationState> destinations;
    private CancellationTokenSource cts;

    public DnsDestinationResolverState(ServerOptions options, List<DestinationConfig> destinationConfigs, ILogger logger)
    {
        this.options = options;
        this.destinationConfigs = destinationConfigs;
        this.logger = logger;
    }

    public DestinationState this[int index] => destinations?[index];

    public int Count => destinations.Count;

    public void Dispose()
    {
        cts?.Cancel();
        cts = null;
        destinations = null;
        destinationConfigs = null;
        options = null;
    }

    public IEnumerator<DestinationState> GetEnumerator()
    {
        return destinations?.GetEnumerator();
    }

    internal async Task LoadAsync(CancellationToken cancellationToken)
    {
        List<DestinationState> destinations = new List<DestinationState>();
        foreach (var item in destinationConfigs)
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
                destinations.AddRange(addresses.Select(i => new DestinationState() { EndPoint = new IPEndPoint(i, port) }));
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Failed to resolve host '{hostName}'. See {nameof(Exception.InnerException)} for details.", exception);
            }
        }

        if (options.DnsRefreshPeriod.HasValue && options.DnsRefreshPeriod > TimeSpan.Zero)
        {
            if (cts != null && !cts.IsCancellationRequested)
            {
                cts.Cancel();
            }
            this.cts = new CancellationTokenSource(options.DnsRefreshPeriod.Value);
            new CancellationChangeToken(cts.Token).RegisterChangeCallback(destinationResolverState =>
            {
                try
                {
                    LoadAsync(new CancellationTokenSource(options.DnsRefreshPeriod.Value).Token).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                }
            }, this);
        }
        this.destinations = destinations;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}