namespace NZ.Orz.Config.Memory;

public sealed record MemoryRouteConfig
{
    public GatewayProtocols Protocols { get; init; } = default!;

    public string RouteId { get; init; } = default!;

    public MemoryRouteMatch Match { get; init; } = default!;

    public int? Order { get; init; }

    public string? ClusterId { get; init; }

    public int? RetryCount { get; init; }

    /// <summary>
    /// tcp : read / write timeout not connection timeout, udp revice response timeout, http ...
    /// </summary>
    public TimeSpan? Timeout { get; init; }

    internal RouteConfig Build(ServerOptions serverOptions)
    {
        return new RouteConfig
        {
            Protocols = Protocols,
            RouteId = RouteId,
            Match = Match?.Build(),
            Order = Order.GetValueOrDefault(),
            ClusterId = ClusterId,
            Timeout = Timeout.GetValueOrDefault(serverOptions.DefaultProxyTimeout),
            RetryCount = RetryCount.GetValueOrDefault()
        };
    }
}