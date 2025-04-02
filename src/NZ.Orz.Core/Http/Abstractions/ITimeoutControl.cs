using NZ.Orz.Config;
using NZ.Orz.Http.Http2.FlowControl;

namespace NZ.Orz.Http;

public interface ITimeoutControl
{
    TimeoutReason TimerReason { get; }

    void SetTimeout(TimeSpan timeout, TimeoutReason timeoutReason);

    void ResetTimeout(TimeSpan timeout, TimeoutReason timeoutReason);

    void CancelTimeout();

    void InitializeHttp2(InputFlowControl connectionInputFlowControl);

    void Tick(long timestamp);

    void StartRequestBody(MinDataRate minRate);

    void StopRequestBody();

    void StartTimingRead();

    void StopTimingRead();

    void BytesRead(long count);

    void StartTimingWrite();

    void StopTimingWrite();

    void BytesWrittenToBuffer(MinDataRate minRate, long count);

    long GetResponseDrainDeadline(long timestamp, MinDataRate minRate);
}