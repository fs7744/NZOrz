using Microsoft.Extensions.Logging;
using NZ.Orz.Config;
using NZ.Orz.Connections;
using System.Net;

namespace NZ.Orz.Sockets;

public sealed class UdpTransportFactory : IConnectionListenerFactory, IConnectionListenerFactorySelector
{
    private readonly IRouteContractor contractor;
    private readonly ILoggerFactory _logger;

    public UdpTransportFactory(
        IRouteContractor contractor,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(contractor);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        this.contractor = contractor;
        _logger = loggerFactory;
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