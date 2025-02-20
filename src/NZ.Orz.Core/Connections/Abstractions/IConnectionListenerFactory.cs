using NZ.Orz.Config;
using System.Net;

namespace NZ.Orz.Connections;

public interface IConnectionListenerFactory
{
    ValueTask<IConnectionListener> BindAsync(EndPoint endpoint, GatewayProtocols protocols, CancellationToken cancellationToken = default);
}