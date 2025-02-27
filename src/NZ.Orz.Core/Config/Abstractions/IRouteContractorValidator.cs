using NZ.Orz.Sockets;

namespace NZ.Orz.Config;

public interface IRouteContractorValidator
{
    public ValueTask<IList<ListenOptions>> ValidateAndGenerateListenOptionsAsync(IProxyConfig config, ServerOptions serverOptions, SocketTransportOptions options);
}