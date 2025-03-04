using NZ.Orz.Config;
using NZ.Orz.Connections;

namespace NZ.Orz.ReverseProxy.LoadBalancing;

public interface ILoadBalancingPolicy
{
    string Name { get; }

    DestinationState? PickDestination(ConnectionContext context, IReadOnlyList<DestinationState> availableDestinations);
}