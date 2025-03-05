using NZ.Orz.Infrastructure;

namespace NZ.Orz.Config;

public sealed record RouteMatch
{
    /// <summary>
    /// tcp / udp will match instances ips:ports, http will match host header
    /// </summary>
    public IReadOnlyList<string>? Hosts { get; init; }

    public bool Equals(RouteMatch? other)
    {
        if (other is null)
        {
            return false;
        }

        return CollectionUtilities.Equals(Hosts, other.Hosts);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(CollectionUtilities.GetStringHashCode(Hosts));
    }
}