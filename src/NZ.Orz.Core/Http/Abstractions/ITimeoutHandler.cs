namespace NZ.Orz.Http;

public interface ITimeoutHandler
{
    void OnTimeout(TimeoutReason reason);
}