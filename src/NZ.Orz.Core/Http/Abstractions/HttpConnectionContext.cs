using NZ.Orz.Config;
using NZ.Orz.Connections;

namespace NZ.Orz.Http;

public abstract class HttpConnectionContext
{
    public HttpConnectionContext(GatewayProtocols protocols, BaseConnectionContext connectionContext)
    {
        Protocols = protocols;
        ConnectionContext = connectionContext;
    }

    public GatewayProtocols Protocols { get; }

    public BaseConnectionContext ConnectionContext { get; }
}