namespace NZ.Orz.Connections.Features;

public interface IConnectionLifetimeFeature
{
    CancellationToken ConnectionClosed { get; set; }

    void Abort();
}