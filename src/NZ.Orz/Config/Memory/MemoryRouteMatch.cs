namespace NZ.Orz.Config.Memory;

public sealed record MemoryRouteMatch
{
    /// <summary>
    /// tcp / udp will match instances ips:ports, http will match host header
    /// </summary>
    public List<string>? Hosts { get; init; }

    internal RouteMatch Build()
    {
        return new RouteMatch { Hosts = Hosts };
    }
}