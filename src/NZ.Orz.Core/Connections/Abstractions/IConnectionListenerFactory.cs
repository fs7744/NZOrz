using System.Net;

namespace NZ.Orz.Connections;

public interface IConnectionListenerFactory
{
    ValueTask<IConnectionListener> BindAsync(EndPoint endpoint, CancellationToken cancellationToken = default);
}