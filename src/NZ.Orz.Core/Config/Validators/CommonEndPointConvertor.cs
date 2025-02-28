using NZ.Orz.Config.Abstractions;
using System.Net;
using System.Net.Sockets;

namespace NZ.Orz.Config.Validators;

public class CommonEndPointConvertor : IEndPointConvertor
{
    public bool TryConvert(string address, out EndPoint endPoint)
    {
        if (IPEndPoint.TryParse(address, out var ip))
        {
            endPoint = ip;
            return true;
        }
        else if (File.Exists(address))
        {
            endPoint = new UnixDomainSocketEndPoint(address);
            return true;
        }
        else if (address.StartsWith("localhost:", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(address.AsSpan(10), out var port)
            && port >= IPEndPoint.MinPort && port <= IPEndPoint.MaxPort)
        {
            endPoint = new IPEndPoint(IPAddress.Loopback, port);
            return true;
        }

        endPoint = null;
        return false;
    }
}