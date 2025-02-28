using NZ.Orz.Config;

namespace NZ.Orz.ReverseProxy.LoadBalancing;

public sealed class LoadBalancingPolicy
{
    public DestinationState? PickDestination(RouteConfig route)
    {
        if (route == null) return null;
        var clusterConfig = route.ClusterConfig;
        if (clusterConfig == null || clusterConfig.DestinationStates == null) return null;
        var states = clusterConfig.DestinationStates;
        if (states.Count == 0) return null;
        if (states.Count == 1) return states[0];

        // todo
        return states[0];
    }
}