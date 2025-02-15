using NZ.Orz.Features;
using System.Net;

namespace NZ.Orz.Connections;

public interface IMultiplexedConnectionListener : IAsyncDisposable
{
    EndPoint EndPoint { get; }

    ValueTask UnbindAsync(CancellationToken cancellationToken = default);

    ValueTask<MultiplexedConnectionContext?> AcceptAsync(IFeatureCollection? features = null, CancellationToken cancellationToken = default);
}