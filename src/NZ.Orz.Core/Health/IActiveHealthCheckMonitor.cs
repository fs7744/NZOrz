using NZ.Orz.Config;

namespace NZ.Orz.Health;

public interface IActiveHealthCheckMonitor
{
    Task CheckHealthAsync(IEnumerable<ClusterConfig> clusters);
}