using NZ.Orz.Config;

namespace NZ.Orz.Health;

public class HealthyAndUnknownDestinationsUpdater : IHealthUpdater
{
    public void UpdateAvailableDestinations(ClusterConfig cluster)
    {
        if (cluster.DestinationStates == null) return;
        var availableDestinations = cluster.DestinationStates.ToList();
        if (cluster.HealthCheck != null)
        {
            availableDestinations = availableDestinations.Where(destination => destination.Health != DestinationHealth.Unhealthy).ToList();
        }
        cluster.AvailableDestinations = availableDestinations;
    }
}