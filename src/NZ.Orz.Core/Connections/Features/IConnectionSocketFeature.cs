using System.Net.Sockets;

namespace NZ.Orz.Connections.Features;

public interface IConnectionSocketFeature
{
    Socket Socket { get; }
}