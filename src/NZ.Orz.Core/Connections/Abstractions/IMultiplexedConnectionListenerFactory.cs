using NZ.Orz.Config;
using NZ.Orz.Features;
using System.Net;

namespace NZ.Orz.Connections;

public interface IMultiplexedConnectionListenerFactory
{
    ValueTask<IMultiplexedConnectionListener> BindAsync(EndPoint endpoint, GatewayProtocols protocols, IFeatureCollection? features = null, CancellationToken cancellationToken = default);
}