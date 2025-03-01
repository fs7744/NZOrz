using NZ.Orz.Config;

namespace NZ.Orz.ServiceDiscovery;

public interface IDestinationResolverState : IReadOnlyList<DestinationState>, IDisposable
{
}