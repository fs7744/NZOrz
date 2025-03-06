namespace NZ.Orz.Config;
public sealed record DestinationConfig
{
    public string Address { get; init; } = default!;

    public string? Host { get; init; }

    public bool Equals(DestinationConfig? other)
    {
        if (other is null)
        {
            return false;
        }

        return string.Equals(Address, other.Address, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Host, other.Host, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            Address?.GetHashCode(StringComparison.OrdinalIgnoreCase),
            Host?.GetHashCode(StringComparison.OrdinalIgnoreCase));
    }
}