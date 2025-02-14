using System.Diagnostics.Metrics;

namespace NZ.Orz.Metrics;

public sealed class OrzMetrics
{
    public const string MeterName = "NZ.Orz.Server";
    private readonly Meter _meter;

    public OrzMetrics(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create(MeterName);
    }
}