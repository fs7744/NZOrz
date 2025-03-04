using NZ.Orz.Infrastructure;
using NZ.Orz.ReverseProxy.LoadBalancing;

namespace NZ.Orz.Config;
public sealed record ClusterConfig
{
    public string ClusterId { get; init; } = default!;

    public string? LoadBalancingPolicy { get; init; }

    public HealthCheckConfig? HealthCheck { get; init; }

    public IReadOnlyList<DestinationConfig>? Destinations { get; init; }

    public IReadOnlyList<DestinationState> DestinationStates { get; internal set; }
    public ILoadBalancingPolicy LoadBalancingPolicyInstance { get; internal set; }

    public bool Equals(ClusterConfig? other)
    {
        if (other is null)
        {
            return false;
        }

        return string.Equals(ClusterId, other.ClusterId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(LoadBalancingPolicy, other.LoadBalancingPolicy, StringComparison.OrdinalIgnoreCase)
            && HealthCheck == other.HealthCheck
            && CollectionUtilities.Equals(Destinations, other.Destinations);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            ClusterId?.GetHashCode(StringComparison.OrdinalIgnoreCase),
            LoadBalancingPolicy?.GetHashCode(StringComparison.OrdinalIgnoreCase),
            HealthCheck,
            CollectionUtilities.GetHashCode(Destinations));
    }
}