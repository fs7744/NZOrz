using System.Net;

namespace NZ.Orz.Connections;

public interface IConnectionFactory
{
    ValueTask<ConnectionContext> ConnectAsync(EndPoint endpoint, CancellationToken cancellationToken = default);
}