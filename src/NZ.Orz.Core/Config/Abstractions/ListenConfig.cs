using NZ.Orz.Infrastructure;

namespace NZ.Orz.Config;

public class ListenConfig
{
    public string ListenId { get; init; }
    public GatewayProtocols Protocols { get; init; }

    public string[]? Address { get; init; }

    public static bool Equals(ListenConfig? t, ListenConfig? other)
    {
        if (t is null && other is null) return true;
        if (other is null)
        {
            return false;
        }

        return string.Equals(t.ListenId, other.ListenId, StringComparison.OrdinalIgnoreCase)
            && t.Protocols == other.Protocols
            && CollectionUtilities.EqualsString(t.Address, other.Address, StringComparer.OrdinalIgnoreCase);
    }

    public bool Equals(ListenConfig? other)
    {
        return Equals(this, other);
    }

    public static int GetHashCode(ListenConfig t)
    {
        return HashCode.Combine(
            t.ListenId?.GetHashCode(StringComparison.OrdinalIgnoreCase),
            t.Protocols,
            CollectionUtilities.GetStringHashCode(t.Address, StringComparer.OrdinalIgnoreCase));
    }

    public override int GetHashCode()
    {
        return GetHashCode(this);
    }
}