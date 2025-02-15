namespace NZ.Orz.Connections;

internal interface IConnectionListener<T> : IConnectionListenerBase where T : BaseConnectionContext
{
    ValueTask<T?> AcceptAsync(CancellationToken cancellationToken = default);
}