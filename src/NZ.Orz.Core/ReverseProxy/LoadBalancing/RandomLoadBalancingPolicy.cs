using NZ.Orz.Config;
using NZ.Orz.Connections;

namespace NZ.Orz.ReverseProxy.LoadBalancing;

public sealed class RandomLoadBalancingPolicy : ILoadBalancingPolicy
{
    public string Name => LoadBalancingPolicy.Random;

    public DestinationState? PickDestination(ConnectionContext context, ClusterConfig cluster)
    {
        var d = cluster.DestinationStates;
        return d[Random.Shared.Next(d.Count)];
    }
}