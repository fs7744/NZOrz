using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZ.Orz.Http;

public interface ITimeoutControl
{
    TimeoutReason TimerReason { get; }

    void SetTimeout(TimeSpan timeout, TimeoutReason timeoutReason);

    void ResetTimeout(TimeSpan timeout, TimeoutReason timeoutReason);

    void CancelTimeout();

    void Tick(long timestamp);
}