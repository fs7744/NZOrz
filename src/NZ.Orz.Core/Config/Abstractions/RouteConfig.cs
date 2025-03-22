using NZ.Orz.Infrastructure;

namespace NZ.Orz.Config;

public sealed record RouteConfig
{
    public GatewayProtocols Protocols { get; init; } = default!;

    public string RouteId { get; init; } = default!;

    public RouteMatch Match { get; init; } = default!;

    public int Order { get; init; }

    public string? ClusterId { get; init; }

    /// <summary>
    /// tcp : read / write timeout not connection timeout, udp revice response timeout, http ...
    /// </summary>
    public TimeSpan Timeout { get; init; }

    public int RetryCount { get; init; }
    public int UdpResponses { get; init; }
    public SslConfig Ssl { get; init; }
    public ClusterConfig ClusterConfig { get; internal set; }

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
            && RetryCount == other.RetryCount
            && UdpResponses == other.UdpResponses
            && Ssl == other.Ssl
            && Match == other.Match;
    }

    public override int GetHashCode()
    {
        var code = new HashCode();
        code.Add(Protocols);
        code.Add(Order);
        code.Add(RouteId?.GetHashCode(StringComparison.OrdinalIgnoreCase));
        code.Add(ClusterId?.GetHashCode(StringComparison.OrdinalIgnoreCase));
        code.Add(Timeout.GetHashCode());
        code.Add(RetryCount);
        code.Add(UdpResponses);
        code.Add(Ssl?.GetHashCode());
        code.Add(Match.GetHashCode());
        return code.ToHashCode();
    }

    internal CancellationTokenSource CreateTimeoutTokenSource(CancellationTokenSourcePool pool)
    {
        var cts = pool.Rent();
        cts.CancelAfter(Timeout);
        return cts;
    }
}