using NZ.Orz.Config;

namespace NZ.Orz.Health;

public interface IActiveHealthChecker
{
    string Name { get; }

    Task CheckAsync(ActiveHealthCheckConfig config, DestinationState state, CancellationToken cancellationToken);
}