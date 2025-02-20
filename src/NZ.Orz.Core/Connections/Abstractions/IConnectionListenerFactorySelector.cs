using NZ.Orz.Config;
using System.Net;

namespace NZ.Orz.Connections;

public interface IConnectionListenerFactorySelector
{
    bool CanBind(EndPoint endpoint, GatewayProtocols protocols);
}