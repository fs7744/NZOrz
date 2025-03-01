using NZ.Orz.Config;
using System.Collections;

namespace NZ.Orz.ServiceDiscovery;

public class FuncDestinationResolverState : IDestinationResolverState
{
    public List<DestinationConfig> Configs { get; private set; }
    private Func<FuncDestinationResolverState, CancellationToken, Task> resolveAsync;
    public CancellationTokenSource CancellationTokenSource { get; set; }
    public IReadOnlyList<DestinationState> Destinations { get; set; }

    public FuncDestinationResolverState(List<DestinationConfig> destinationConfigs, Func<FuncDestinationResolverState, CancellationToken, Task> resolveAsync)
    {
        this.Configs = destinationConfigs;
        this.resolveAsync = resolveAsync;
    }

    public DestinationState this[int index] => Destinations?[index];

    public int Count => Destinations.Count;

    public void Dispose()
    {
        CancellationTokenSource?.Cancel();
        CancellationTokenSource = null;
        Destinations = null;
        Configs = null;
    }

    public IEnumerator<DestinationState> GetEnumerator()
    {
        return Destinations?.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public async Task ResolveAsync(CancellationToken cancellationToken)
    {
        await resolveAsync(this, cancellationToken);
    }
}