namespace NZ.Orz.Config.Memory;
public sealed record MemoryClusterConfig
{
    public string ClusterId { get; init; } = default!;

    public string? LoadBalancingPolicy { get; init; }

    public HealthCheckConfig? HealthCheck { get; init; }

    public List<DestinationConfig>? Destinations { get; init; }

    internal ClusterConfig Build()
    {
        return new ClusterConfig() { ClusterId = ClusterId, LoadBalancingPolicy = LoadBalancingPolicy, HealthCheck = HealthCheck, Destinations = Destinations };
    }
}