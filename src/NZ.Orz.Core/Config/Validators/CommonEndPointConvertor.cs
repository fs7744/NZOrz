using System.Net;
using System.Net.Sockets;

namespace NZ.Orz.Config.Validators;

public class CommonEndPointConvertor : IEndPointConvertor
{
    public int Order => 0;

    public bool TryConvert(string address, out IEnumerable<EndPoint> endPoint)
    {
        if (IPEndPoint.TryParse(address, out var ip))
        {
            endPoint = [ip];
            return true;
        }
        else if (File.Exists(address))
        {
            endPoint = [new UnixDomainSocketEndPoint(address)];
            return true;
        }
        else if (address.StartsWith("localhost:", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(address.AsSpan(10), out var port)
            && port >= IPEndPoint.MinPort && port <= IPEndPoint.MaxPort)
        {
            endPoint = [new IPEndPoint(IPAddress.Loopback, port), new IPEndPoint(IPAddress.IPv6Loopback, port)];
            return true;
        }
        else if (address.StartsWith("*:")
            && int.TryParse(address.AsSpan(2), out port)
            && port >= IPEndPoint.MinPort && port <= IPEndPoint.MaxPort)
        {
            endPoint = [new IPEndPoint(IPAddress.Any, port), new IPEndPoint(IPAddress.IPv6Any, port)];
            return true;
        }

        endPoint = null;
        return false;
    }
}