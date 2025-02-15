using NZ.Orz.Features;
using System.Net;

namespace NZ.Orz.Connections;

public interface IMultiplexedConnectionListenerFactory
{
    ValueTask<IMultiplexedConnectionListener> BindAsync(EndPoint endpoint, IFeatureCollection? features = null, CancellationToken cancellationToken = default);
}