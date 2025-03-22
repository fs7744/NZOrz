using NZ.Orz.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZ.Orz.Http;

public sealed class TimeoutControl : ITimeoutControl
{
    private readonly ITimeoutHandler _timeoutHandler;
    private readonly TimeProvider _timeProvider;
    private readonly long _heartbeatIntervalTicks;
    private long _lastTimestamp;

    public TimeoutControl(ITimeoutHandler timeoutHandler, TimeProvider timeProvider)
    {
        _timeoutHandler = timeoutHandler;
        _timeProvider = timeProvider;
        _heartbeatIntervalTicks = Heartbeat.Interval.ToTicks(_timeProvider);
    }

    public TimeoutReason TimerReason { get; private set; }

    public void Initialize()
    {
        Interlocked.Exchange(ref _lastTimestamp, _timeProvider.GetTimestamp());
    }

    public void CancelTimeout()
    {
        throw new NotImplementedException();
    }

    public void ResetTimeout(TimeSpan timeout, TimeoutReason timeoutReason)
    {
        throw new NotImplementedException();
    }

    public void SetTimeout(TimeSpan timeout, TimeoutReason timeoutReason)
    {
        throw new NotImplementedException();
    }

    public void Tick(long timestamp)
    {
        throw new NotImplementedException();
    }
}