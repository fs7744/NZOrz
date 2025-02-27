using NZ.Orz.Sockets;

namespace NZ.Orz.Config;

public interface ISocketTransportOptionsValidator
{
    public ValueTask ValidateAsync(SocketTransportOptions options, IList<Exception> errors);
}