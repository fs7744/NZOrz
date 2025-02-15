namespace NZ.Orz.Connections.Features;

public interface IConnectionHeartbeatFeature
{
    void OnHeartbeat(Action<object> action, object state);
}