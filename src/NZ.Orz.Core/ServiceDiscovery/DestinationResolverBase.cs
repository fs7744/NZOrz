using NZ.Orz.Config;

namespace NZ.Orz.ServiceDiscovery;

public abstract class DestinationResolverBase : IDestinationResolver
{
    public abstract int Order { get; }

    public async Task<IDestinationResolverState> ResolveDestinationsAsync(List<DestinationConfig> destinationConfigs, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var r = new FuncDestinationResolverState(destinationConfigs, ResolveAsync);
        await r.ResolveAsync(cancellationToken);
        return r;
    }

    public abstract Task ResolveAsync(FuncDestinationResolverState state, CancellationToken cancellationToken);
}