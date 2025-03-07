using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Metrics;
using System.Net;

namespace NZ.Orz.Sockets;

public sealed class UdpTransportFactory : IConnectionListenerFactory, IConnectionListenerFactorySelector
{
    private readonly IRouteContractor contractor;
    private readonly OrzLogger _logger;

    public UdpTransportFactory(
        IRouteContractor contractor,
        OrzLogger logger)
    {
        ArgumentNullException.ThrowIfNull(contractor);
        ArgumentNullException.ThrowIfNull(logger);

        this.contractor = contractor;
        _logger = logger;
    }

    public ValueTask<IConnectionListener> BindAsync(EndPoint endpoint, GatewayProtocols protocols, CancellationToken cancellationToken = default)
    {
        var transport = new UdpConnectionListener(endpoint, GatewayProtocols.UDP, contractor, _logger);
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