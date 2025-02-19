using System.Net;

namespace NZ.Orz.Connections;

public class UdpEndPoint : IPEndPoint
{
    public UdpEndPoint(long address, int port) : base(address, port)
    {
    }

    public UdpEndPoint(IPAddress address, int port) : base(address, port)
    {
    }
}