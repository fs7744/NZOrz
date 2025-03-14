using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Metrics;
using System.Net;
using System.Net.Sockets;

namespace NZ.Orz.Sockets;

public sealed class SocketTransportFactory : IConnectionListenerFactory, IConnectionListenerFactorySelector
{
    private readonly IRouteContractor contractor;
    private readonly OrzLogger _logger;

    public SocketTransportFactory(
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
        var transport = new SocketConnectionListener(endpoint, protocols, contractor, _logger);
        transport.Bind();
        return new ValueTask<IConnectionListener>(transport);
    }

    public bool CanBind(EndPoint endpoint, GatewayProtocols protocols)
    {
        if (protocols is not (GatewayProtocols.TCP or GatewayProtocols.SNI)) return false;
        return endpoint switch
        {
            IPEndPoint _ => true,
            UnixDomainSocketEndPoint _ => true,
            _ => false
        };
    }
}