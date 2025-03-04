using NZ.Orz.Config;

namespace NZ.Orz.Health;

public interface IHealthUpdater
{
    void UpdateAvailableDestinations(ClusterConfig cluster);
}