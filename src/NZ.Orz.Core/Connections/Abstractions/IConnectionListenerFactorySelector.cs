using System.Net;

namespace NZ.Orz.Connections;

public interface IConnectionListenerFactorySelector
{
    bool CanBind(EndPoint endpoint);
}