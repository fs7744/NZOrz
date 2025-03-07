using NZ.Orz.Config;
using NZ.Orz.Connections;

namespace NZ.Orz.ReverseProxy.LoadBalancing;

public sealed class LoadBalancingPolicy
{
    public static string Random => nameof(Random);
    public static string RoundRobin => nameof(RoundRobin);
    public static string LeastRequests => nameof(LeastRequests);
    public static string PowerOfTwoChoices => nameof(PowerOfTwoChoices);

    public DestinationState? PickDestination(ConnectionContext context, RouteConfig route)
    {
        if (route is null) return null;
        var clusterConfig = route.ClusterConfig;
        if (clusterConfig is null || clusterConfig.AvailableDestinations is null) return null;
        var states = clusterConfig.AvailableDestinations;
        if (states is null || states.Count == 0) return null;
        if (states.Count == 1) return states[0];

        return clusterConfig.LoadBalancingPolicyInstance.PickDestination(context, states);
    }
}