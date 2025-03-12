using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Metrics;
using NZ.Orz.Sockets.Client;
using System.Net;

namespace NZ.Orz.Sockets;

public sealed class UdpTransportFactory : IConnectionListenerFactory, IConnectionListenerFactorySelector
{
    private readonly IRouteContractor contractor;
    private readonly OrzLogger logger;
    private readonly IUdpConnectionFactory connectionFactory;

    public UdpTransportFactory(
        IRouteContractor contractor,
        OrzLogger logger,
        IUdpConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(contractor);
        ArgumentNullException.ThrowIfNull(logger);

        this.contractor = contractor;
        this.logger = logger;
        this.connectionFactory = connectionFactory;
    }

    public ValueTask<IConnectionListener> BindAsync(EndPoint endpoint, GatewayProtocols protocols, CancellationToken cancellationToken = default)
    {
        var transport = new UdpConnectionListener(endpoint, GatewayProtocols.UDP, contractor, logger, connectionFactory);
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