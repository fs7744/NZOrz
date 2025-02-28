using NZ.Orz.Infrastructure;
using System.Net;

namespace NZ.Orz.Config;

public class DestinationState
{
    public EndPoint? EndPoint { get; set; }

    public int ConcurrentRequestCount
    {
        get => ConcurrencyCounter.Value;
        set => ConcurrencyCounter.Value = value;
    }

    internal AtomicCounter ConcurrencyCounter { get; } = new AtomicCounter();
}