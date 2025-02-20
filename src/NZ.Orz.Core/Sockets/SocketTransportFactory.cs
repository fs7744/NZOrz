using Microsoft.Extensions.Logging;
using NZ.Orz.Config;
using NZ.Orz.Connections;
using System.Net;
using System.Net.Sockets;

namespace NZ.Orz.Sockets;

public sealed class SocketTransportFactory : IConnectionListenerFactory, IConnectionListenerFactorySelector
{
    private readonly IRouteContractor contractor;
    private readonly ILoggerFactory _logger;

    public SocketTransportFactory(
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
        var transport = new SocketConnectionListener(endpoint, protocols, contractor, _logger);
        transport.Bind();
        return new ValueTask<IConnectionListener>(transport);
    }

    public bool CanBind(EndPoint endpoint, GatewayProtocols protocols)
    {
        if (!protocols.HasFlag(GatewayProtocols.TCP)) return false;
        return endpoint switch
        {
            IPEndPoint _ => true,
            UnixDomainSocketEndPoint _ => true,
            _ => false
        };
    }
}