namespace NZ.Orz.Connections.Features;

public interface IConnectionMetricsContextFeature
{
    ConnectionMetricsContext MetricsContext { get; }
}

public sealed class ConnectionMetricsContext
{
    public BaseConnectionContext ConnectionContext { get; init; }
    public bool CurrentConnectionsCounterEnabled { get; init; }
    public bool ConnectionDurationEnabled { get; init; }
    public bool QueuedConnectionsCounterEnabled { get; init; }
    public bool QueuedRequestsCounterEnabled { get; init; }
    public bool CurrentUpgradedRequestsCounterEnabled { get; init; }
    public bool CurrentTlsHandshakesCounterEnabled { get; init; }

    public ConnectionEndReason? ConnectionEndReason { get; set; }
}