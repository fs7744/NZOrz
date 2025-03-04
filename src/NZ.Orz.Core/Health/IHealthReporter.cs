using NZ.Orz.Config;

namespace NZ.Orz.Health;

public interface IHealthReporter
{
    void ReportFailed(DestinationState destinationState);

    void ReportSuccessed(DestinationState destinationState);
}