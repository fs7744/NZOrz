using NZ.Orz.Config;
using NZ.Orz.Connections;

namespace NZ.Orz.ReverseProxy.LoadBalancing;

public sealed class LeastRequestsLoadBalancingPolicy : ILoadBalancingPolicy
{
    public string Name => LoadBalancingPolicy.LeastRequests;

    public DestinationState? PickDestination(ConnectionContext context, IReadOnlyList<DestinationState> availableDestinations)
    {
        var destinationCount = availableDestinations.Count;
        var leastRequestsDestination = availableDestinations[0];
        var leastRequestsCount = leastRequestsDestination.ConcurrentRequestCount;
        for (var i = 1; i < destinationCount; i++)
        {
            var destination = availableDestinations[i];
            var endpointRequestCount = destination.ConcurrentRequestCount;
            if (endpointRequestCount < leastRequestsCount)
            {
                leastRequestsDestination = destination;
                leastRequestsCount = endpointRequestCount;
            }
        }
        return leastRequestsDestination;
    }
}