﻿namespace NZ.Orz.Config;

public sealed record RouteConfig
{
    public GatewayProtocols Protocols { get; init; } = default!;

    public string RouteId { get; init; } = default!;

    public RouteMatch Match { get; init; } = default!;

    public int? Order { get; init; }

    public string? ClusterId { get; init; }

    /// <summary>
    /// tcp : read / write timeout not connection timeout, udp revice response timeout, http ...
    /// </summary>
    public TimeSpan? Timeout { get; init; }

    public bool Equals(RouteConfig? other)
    {
        if (other is null)
        {
            return false;
        }

        return Protocols == other.Protocols
            && Order == other.Order
            && string.Equals(RouteId, other.RouteId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(ClusterId, other.ClusterId, StringComparison.OrdinalIgnoreCase)
            && Timeout == other.Timeout
            && Match == other.Match;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Protocols,
            Order,
            RouteId?.GetHashCode(StringComparison.OrdinalIgnoreCase),
            ClusterId?.GetHashCode(StringComparison.OrdinalIgnoreCase),
            Timeout?.GetHashCode(),
            Match);
    }
}