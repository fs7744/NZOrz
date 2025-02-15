namespace NZ.Orz.Connections.Features;

public interface IConnectionCompleteFeature
{
    void OnCompleted(Func<object, Task> callback, object state);
}