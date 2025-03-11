using NZ.Orz.Buffers;
using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Metrics;
using System.Buffers;
using System.Net;

namespace NZ.Orz.Sockets;

public sealed class UdpTransportFactory : IConnectionListenerFactory, IConnectionListenerFactorySelector
{
    private readonly IRouteContractor contractor;
    private readonly OrzLogger _logger;
    private readonly MemoryPool<byte> _pool;

    public UdpTransportFactory(
        IRouteContractor contractor,
        OrzLogger logger)
    {
        ArgumentNullException.ThrowIfNull(contractor);
        ArgumentNullException.ThrowIfNull(logger);

        this.contractor = contractor;
        _logger = logger;
        var options = contractor.GetSocketTransportOptions();
        _pool = PinnedBlockMemoryPoolFactory.Create(options.UdpMaxSize);
    }

    public ValueTask<IConnectionListener> BindAsync(EndPoint endpoint, GatewayProtocols protocols, CancellationToken cancellationToken = default)
    {
        var transport = new UdpConnectionListener(endpoint, GatewayProtocols.UDP, contractor, _logger, _pool);
        transport.Bind();
        return new ValueTask<IConnectionListener>(transport);
    }

    public bool CanBind(EndPoint endpoint, GatewayProtocols protocols)
    {
        if (!protocols.HasFlag(GatewayProtocols.UDP)) return false;
        return endpoint switch
        {
            IPEndPoint _ => true,
            _ => false
        };
    }
}