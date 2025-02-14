using System.Net;

namespace NZ.Orz.Connections;

public interface IConnectionListenerFactory
{
    bool CanBind(EndPoint endpoint);

    ValueTask<IConnectionListener> BindAsync(EndPoint endpoint, CancellationToken cancellationToken = default);
}