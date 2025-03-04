using NZ.Orz.Sockets;

namespace NZ.Orz.Config;

public interface ISocketTransportOptionsValidator
{
    int Order { get; }

    public ValueTask ValidateAsync(SocketTransportOptions options, IList<Exception> errors, CancellationToken cancellationToken);
}