using NZ.Orz.Config;
using System.Collections;

namespace NZ.Orz.ServiceDiscovery;

public class StaticDestinationResolverState : IDestinationResolverState
{
    private readonly IReadOnlyList<DestinationState> states;

    public StaticDestinationResolverState(IReadOnlyList<DestinationState> states)
    {
        this.states = states;
    }

    public DestinationState this[int index] => states[index];

    public int Count => states.Count;

    public void Dispose()
    {
    }

    public IEnumerator<DestinationState> GetEnumerator()
    {
        return states.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return states.GetEnumerator();
    }
}