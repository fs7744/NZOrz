using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Infrastructure;
using System.Runtime.CompilerServices;

namespace NZ.Orz.ReverseProxy.LoadBalancing;

public sealed class RoundRobinLoadBalancingPolicy : ILoadBalancingPolicy
{
    private readonly ConditionalWeakTable<RouteConfig, AtomicCounter> _counters = new();
    public string Name => LoadBalancingPolicy.RoundRobin;

    public DestinationState? PickDestination(ConnectionContext context, IReadOnlyList<DestinationState> availableDestinations)
    {
        if (availableDestinations.Count == 0)
        {
            return null;
        }

        var counter = _counters.GetOrCreateValue(context.Route);

        // Increment returns the new value and we want the first return value to be 0.
        var offset = counter.Increment() - 1;

        // Preventing negative indices from being computed by masking off sign.
        // Ordering of index selection is consistent across all offsets.
        // There may be a discontinuity when the sign of offset changes.
        return availableDestinations[(offset & 0x7FFFFFFF) % availableDestinations.Count];
    }
}