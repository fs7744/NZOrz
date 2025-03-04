using NZ.Orz.Health;
using NZ.Orz.Infrastructure;
using System.Net;

namespace NZ.Orz.Config;

public class DestinationState : IDisposable
{
    public EndPoint? EndPoint { get; set; }

    public int ConcurrentRequestCount
    {
        get => ConcurrencyCounter.Value;
        set => ConcurrencyCounter.Value = value;
    }

    internal AtomicCounter ConcurrencyCounter { get; } = new AtomicCounter();

    internal ClusterConfig ClusterConfig { get; set; }

    public DestinationHealth Health { get; set; }

    public void Dispose()
    {
        ClusterConfig = null;
    }

    internal void ReportFailed()
    {
        ClusterConfig?.HealthReporter?.ReportFailed(this);
    }

    internal void ReportSuccessed()
    {
        ClusterConfig?.HealthReporter?.ReportSuccessed(this);
    }
}