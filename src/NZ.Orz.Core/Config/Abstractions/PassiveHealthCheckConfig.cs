namespace NZ.Orz.Config;
public sealed record PassiveHealthCheckConfig
{
    public bool? Enabled { get; init; }

    public string? Policy { get; init; }

    public TimeSpan? ReactivationPeriod { get; init; }

    public bool Equals(PassiveHealthCheckConfig? other)
    {
        if (other is null)
        {
            return false;
        }

        return Enabled == other.Enabled
            && string.Equals(Policy, other.Policy, StringComparison.OrdinalIgnoreCase)
            && ReactivationPeriod == other.ReactivationPeriod;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Enabled,
            Policy?.GetHashCode(StringComparison.OrdinalIgnoreCase),
            ReactivationPeriod);
    }
}