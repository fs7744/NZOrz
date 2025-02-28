using System.Net;
using System.Net.Sockets;

namespace NZ.Orz.Config.Validators;

public class ClusterConfigValidator : IClusterConfigValidator
{
    public async ValueTask ValidateAsync(ClusterConfig cluster, IList<Exception> errors)
    {
        var destinationStates = new List<DestinationState>();
        foreach (var d in cluster.Destinations)
        {
            var address = d.Address;
            if (IPEndPoint.TryParse(address, out var ip))
            {
                destinationStates.Add(new DestinationState() { EndPoint = ip });
            }
            else if (File.Exists(address))
            {
                destinationStates.Add(new DestinationState() { EndPoint = new UnixDomainSocketEndPoint(address) });
            }
            else if (address.StartsWith("localhost:", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(address.AsSpan(10), out var port)
                && port >= IPEndPoint.MinPort && port <= IPEndPoint.MaxPort)
            {
                destinationStates.Add(new DestinationState() { EndPoint = new IPEndPoint(IPAddress.Loopback, port) });
                destinationStates.Add(new DestinationState() { EndPoint = new IPEndPoint(IPAddress.IPv6Loopback, port) });
            }
            else
            {
                errors.Add(new NotSupportedException($"Not supported destination EndPoint {address}"));
            }
        }
        if (destinationStates.Count > 0)
        {
            cluster.DestinationStates = destinationStates;
        }
    }
}