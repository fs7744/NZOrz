using NZ.Orz.Features;
using System.Net;

namespace NZ.Orz.Connections;

public interface IMultiplexedConnectionFactory
{
    ValueTask<MultiplexedConnectionContext> ConnectAsync(EndPoint endpoint, IFeatureCollection? features = null, CancellationToken cancellationToken = default);
}