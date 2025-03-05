namespace NZ.Orz.Config;

public sealed record ActiveHealthCheckConfig
{
    public TimeSpan Interval { get; init; } = TimeSpan.FromSeconds(15);

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(10);

    public string? Policy { get; init; } = "Tcp";

    /// <summary>
    /// HTTP health check endpoint path.
    /// </summary>
    public string? Path { get; init; }

    /// <summary>
    /// Query string to append to the probe, including the leading '?'.
    /// </summary>
    public string? Query { get; init; }

    public bool Equals(ActiveHealthCheckConfig? other)
    {
        if (other is null)
        {
            return false;
        }

        return Interval == other.Interval
            && Timeout == other.Timeout
            && string.Equals(Policy, other.Policy, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Path, other.Path, StringComparison.Ordinal)
            && string.Equals(Query, other.Query, StringComparison.Ordinal);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Interval,
            Timeout,
            Policy?.GetHashCode(StringComparison.OrdinalIgnoreCase),
            Path?.GetHashCode(StringComparison.Ordinal),
            Query?.GetHashCode(StringComparison.Ordinal));
    }
}