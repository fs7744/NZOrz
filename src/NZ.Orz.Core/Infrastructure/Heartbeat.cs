using NZ.Orz.Metrics;

namespace NZ.Orz.Infrastructure;

public interface IHeartbeatHandler
{
    void OnHeartbeat();
}

public class Heartbeat : IDisposable
{
    public static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

    private readonly IHeartbeatHandler[] _callbacks;
    private readonly TimeProvider _timeProvider;
    private readonly OrzLogger _trace;
    private readonly TimeSpan _interval;
    private readonly Thread _timerThread;
    private readonly ManualResetEventSlim _stopEvent;

    public Heartbeat(IHeartbeatHandler[] callbacks, TimeProvider timeProvider, OrzLogger trace, TimeSpan interval)
    {
        _callbacks = callbacks;
        _timeProvider = timeProvider;
        _trace = trace;
        _interval = interval;
        _stopEvent = new ManualResetEventSlim(false, spinCount: 0);
        _timerThread = new Thread(state => ((Heartbeat)state!).TimerLoop())
        {
            Name = "NZ.Orz Timer",
            IsBackground = true
        };
    }

    public void Start()
    {
        OnHeartbeat();
        _timerThread.Start(this);
    }

    internal void OnHeartbeat()
    {
        var now = _timeProvider.GetTimestamp();

        try
        {
            foreach (var callback in _callbacks)
            {
                callback.OnHeartbeat();
            }
        }
        catch (Exception ex)
        {
            _trace.UnexpectedException($"{nameof(Heartbeat)}.{nameof(OnHeartbeat)}", ex);
        }
    }

    private void TimerLoop()
    {
        while (!_stopEvent.Wait(_interval))
        {
            OnHeartbeat();
        }
    }

    public void Dispose()
    {
        _stopEvent.Set();

        if (_timerThread.IsAlive)
        {
            _timerThread.Join();
        }

        _stopEvent.Dispose();
    }
}