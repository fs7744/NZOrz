namespace NZ.Orz.Config;
public sealed record PassiveHealthCheckConfig
{
    public TimeSpan DetectionWindowSize { get; set; } = TimeSpan.FromSeconds(60);
    public int MinimalTotalCountThreshold { get; set; } = 10;
    public double FailureRateLimit { get; set; } = 0.3;
    public TimeSpan ReactivationPeriod { get; set; } = TimeSpan.FromSeconds(60);

    public bool Equals(PassiveHealthCheckConfig? other)
    {
        if (other is null)
        {
            return false;
        }

        return DetectionWindowSize == other.DetectionWindowSize
            && MinimalTotalCountThreshold == other.MinimalTotalCountThreshold
            && FailureRateLimit == other.FailureRateLimit
            && ReactivationPeriod == other.ReactivationPeriod;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(DetectionWindowSize,
            MinimalTotalCountThreshold,
            FailureRateLimit,
            ReactivationPeriod);
    }
}