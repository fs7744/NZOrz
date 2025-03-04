using NZ.Orz.Config;
using NZ.Orz.Connections;
using System.Collections.Frozen;

namespace NZ.Orz.ReverseProxy.LoadBalancing;

public sealed class LoadBalancingPolicy
{
    public static string FirstAlphabetical => nameof(FirstAlphabetical);
    public static string Random => nameof(Random);
    public static string RoundRobin => nameof(RoundRobin);
    public static string LeastRequests => nameof(LeastRequests);
    public static string PowerOfTwoChoices => nameof(PowerOfTwoChoices);

    private readonly IReadOnlyDictionary<string, ILoadBalancingPolicy> policies;

    public LoadBalancingPolicy(IEnumerable<ILoadBalancingPolicy> policies)
    {
        this.policies = policies.ToFrozenDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
    }

    public DestinationState? PickDestination(ConnectionContext context, RouteConfig route)
    {
        if (route == null) return null;
        var clusterConfig = route.ClusterConfig;
        if (clusterConfig == null || clusterConfig.DestinationStates == null) return null;
        var states = clusterConfig.DestinationStates;
        if (states.Count == 0) return null;
        if (states.Count == 1) return states[0];

        if (policies.TryGetValue(clusterConfig.LoadBalancingPolicy ?? Random, out var policy))
        {
            return policy.PickDestination(context, clusterConfig);
        }
        return null;
    }
}