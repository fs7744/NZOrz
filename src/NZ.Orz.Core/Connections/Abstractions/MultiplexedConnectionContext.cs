using NZ.Orz.Features;

namespace NZ.Orz.Connections;

public abstract class MultiplexedConnectionContext : BaseConnectionContext, IAsyncDisposable
{
    public abstract ValueTask<ConnectionContext?> AcceptAsync(CancellationToken cancellationToken = default);

    public abstract ValueTask<ConnectionContext> ConnectAsync(IFeatureCollection? features = null, CancellationToken cancellationToken = default);
}