using System.Net;

namespace NZ.Orz.Connections;

internal interface IConnectionListenerBase : IAsyncDisposable
{
    EndPoint EndPoint { get; }

    ValueTask UnbindAsync(CancellationToken cancellationToken = default);
}