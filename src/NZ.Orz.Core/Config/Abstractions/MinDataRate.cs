using NZ.Orz.Infrastructure;

namespace NZ.Orz.Config;

public class MinDataRate
{
    public MinDataRate(double bytesPerSecond, TimeSpan gracePeriod)
    {
        if (bytesPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bytesPerSecond), "Value must be a positive number. To disable a minimum data rate, use null where a MinDataRate instance is expected.");
        }

        if (gracePeriod <= Heartbeat.Interval)
        {
            throw new ArgumentOutOfRangeException(nameof(gracePeriod), $"The request body rate enforcement grace period must be greater than {Heartbeat.Interval.TotalSeconds} second.");
        }

        BytesPerSecond = bytesPerSecond;
        GracePeriod = gracePeriod;
    }

    public double BytesPerSecond { get; }

    public TimeSpan GracePeriod { get; }

    public override string ToString()
    {
        return $"Bytes per second: {BytesPerSecond}, Grace Period: {GracePeriod}";
    }
}