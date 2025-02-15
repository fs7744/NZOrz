namespace NZ.Orz.Config;

public class ServerLimits
{
    private long? _maxConcurrentConnections;

    public long? MaxConcurrentConnections
    {
        get => _maxConcurrentConnections;
        set
        {
            if (value.HasValue && value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be null or a non-negative number.");
            }
            _maxConcurrentConnections = value;
        }
    }

    private long? _maxConcurrentUpgradedConnections;

    public long? MaxConcurrentUpgradedConnections
    {
        get => _maxConcurrentUpgradedConnections;
        set
        {
            if (value.HasValue && value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be null or a non-negative number.");
            }
            _maxConcurrentUpgradedConnections = value;
        }
    }
}