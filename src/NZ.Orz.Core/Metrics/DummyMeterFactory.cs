using System.Diagnostics.Metrics;

namespace NZ.Orz.Metrics;

public sealed class DummyMeterFactory : IMeterFactory
{
    public Meter Create(MeterOptions options) => new Meter(options);

    public void Dispose()
    { }
}