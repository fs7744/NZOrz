﻿using NZ.Orz.Health;
using NZ.Orz.Infrastructure;
using NZ.Orz.ReverseProxy.LoadBalancing;

namespace NZ.Orz.Config;
public sealed record class ClusterConfig : IDisposable
{
    public string ClusterId { get; init; } = default!;

    public string? LoadBalancingPolicy { get; init; }

    public HealthCheckConfig? HealthCheck { get; init; }

    public IReadOnlyList<DestinationConfig>? Destinations { get; init; }
    public IReadOnlyList<DestinationState> DestinationStates { get; internal set; }

    public ILoadBalancingPolicy LoadBalancingPolicyInstance { get; internal set; }
    public List<DestinationState> AvailableDestinations { get; internal set; }
    public IHealthReporter HealthReporter { get; internal set; }

    public void Dispose()
    {
        DestinationStates = null;
        LoadBalancingPolicyInstance = null;
    }

    public bool Equals(ClusterConfig? other)
    {
        if (other is null)
        {
            return false;
        }

        return string.Equals(ClusterId, other.ClusterId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(LoadBalancingPolicy, other.LoadBalancingPolicy, StringComparison.OrdinalIgnoreCase)
            && HealthCheckConfig.Equals(HealthCheck, other.HealthCheck)
            && CollectionUtilities.Equals(Destinations, other.Destinations, DestinationConfig.Comparer);
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