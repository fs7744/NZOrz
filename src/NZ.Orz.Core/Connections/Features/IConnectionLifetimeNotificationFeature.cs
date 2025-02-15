namespace NZ.Orz.Connections.Features;

public interface IConnectionLifetimeNotificationFeature
{
    CancellationToken ConnectionClosedRequested { get; set; }

    void RequestClose();
}