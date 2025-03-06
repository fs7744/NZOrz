namespace NZ.Orz.Config;

public sealed record ActiveHealthCheckConfig
{
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(15);

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

    public string? Policy { get; set; } = "Tcp";

    public int Passes { get; set; } = 1;

    public int Fails { get; set; } = 1;

    /// <summary>
    /// HTTP health check endpoint path.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Query string to append to the probe, including the leading '?'.
    /// </summary>
    public string? Query { get; set; }

    public bool Equals(ActiveHealthCheckConfig? other)
    {
        if (other is null)
        {
            return false;
        }

        return Interval == other.Interval
            && Timeout == other.Timeout
            && Passes == other.Passes
            && Fails == other.Fails
            && string.Equals(Policy, other.Policy, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Path, other.Path, StringComparison.Ordinal)
            && string.Equals(Query, other.Query, StringComparison.Ordinal);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Interval,
            Timeout,
            Passes,
            Fails,
            Policy?.GetHashCode(StringComparison.OrdinalIgnoreCase),
            Path?.GetHashCode(StringComparison.Ordinal),
            Query?.GetHashCode(StringComparison.Ordinal));
    }
}