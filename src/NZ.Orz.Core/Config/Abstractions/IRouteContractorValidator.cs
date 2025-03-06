using NZ.Orz.Sockets;

namespace NZ.Orz.Config;

public interface IRouteContractorValidator
{
    int Order { get; }

    public ValueTask<IList<ListenOptions>> ValidateAndGenerateListenOptionsAsync(ProxyConfigSnapshot config, ServerOptions serverOptions, SocketTransportOptions options, IList<Exception> errors, CancellationToken cancellationToken);

    Task ValidateSystemConfigAsync(ServerOptions serverOptions, SocketTransportOptions socketTransportOptions, CancellationToken cancellationToken);
}