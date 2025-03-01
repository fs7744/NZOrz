using NZ.Orz.Config;

namespace NZ.Orz.ServiceDiscovery;

public interface IDestinationResolver
{
    Task<IDestinationResolverState> ResolveDestinationsAsync(List<DestinationConfig> destinationConfigs, CancellationToken cancellationToken);
}