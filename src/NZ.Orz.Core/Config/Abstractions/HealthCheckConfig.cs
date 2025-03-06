using NZ.Orz.Health;

namespace NZ.Orz.Config;

public sealed record HealthCheckConfig
{
    public PassiveHealthCheckConfig? Passive { get; init; }

    public ActiveHealthCheckConfig? Active { get; init; }

    public bool Equals(HealthCheckConfig? other)
    {
        if (other is null)
        {
            return false;
        }

        return Passive == other.Passive
            && Active == other.Active;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            Passive,
            Active);
    }
}