﻿using NZ.Orz.Http.Exceptions;
using NZ.Orz.Metrics;
using System.Globalization;
using System.IO.Pipelines;

namespace NZ.Orz.Http;

public abstract class MessageBody
{
    public static readonly MessageBody ZeroContentLengthClose = new ZeroContentLengthMessageBody(keepAlive: false);
    public static readonly MessageBody ZeroContentLengthKeepAlive = new ZeroContentLengthMessageBody(keepAlive: true);

    protected readonly HttpProtocol _context;
    private bool _send100Continue = true;
    private long _observedBytes;
    private bool _stopped;
    protected bool _timingEnabled;
    protected bool _backpressure;
    protected long _alreadyTimedBytes;
    protected long _examinedUnconsumedBytes;

    protected MessageBody(HttpProtocol context)
    {
        _context = context;
    }

    public HttpProtocol Context => _context;
    public bool RequestKeepAlive { get; protected set; }

    public bool RequestUpgrade { get; protected set; }

    public bool ExtendedConnect { get; protected set; }
    public virtual bool IsEmpty => false;

    protected OrzLogger Log => _context.ServiceContext.Log;

    public abstract ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default);

    public abstract bool TryRead(out ReadResult readResult);

    public void AdvanceTo(SequencePosition consumed)
    {
        AdvanceTo(consumed, consumed);
    }

    public abstract void AdvanceTo(SequencePosition consumed, SequencePosition examined);

    public abstract void CancelPendingRead();

    public abstract void Complete(Exception? exception);

    public virtual ValueTask CompleteAsync(Exception? exception)
    {
        Complete(exception);
        return default;
    }

    public virtual Task ConsumeAsync()
    {
        Task startTask = TryStartAsync();
        if (!startTask.IsCompletedSuccessfully)
        {
            return ConsumeAwaited(startTask);
        }

        return OnConsumeAsync();
    }

    private async Task ConsumeAwaited(Task startTask)
    {
        await startTask;
        await OnConsumeAsync();
    }

    public virtual ValueTask StopAsync()
    {
        TryStop();

        return OnStopAsync();
    }

    protected virtual Task OnConsumeAsync() => Task.CompletedTask;

    protected virtual ValueTask OnStopAsync() => default;

    public virtual void Reset()
    {
        _send100Continue = true;
        _observedBytes = 0;
        _stopped = false;
        _timingEnabled = false;
        _backpressure = false;
        _alreadyTimedBytes = 0;
        _examinedUnconsumedBytes = 0;
    }

    protected ValueTask<FlushResult> TryProduceContinueAsync()
    {
        //todo
        //if (_send100Continue)
        //{
        //    _send100Continue = false;
        //    return _context.HttpResponseControl.ProduceContinueAsync();
        //}

        return default;
    }

    protected Task TryStartAsync()
    {
        if (_context.HasStartedConsumingRequestBody)
        {
            return Task.CompletedTask;
        }

        OnReadStarting();
        _context.HasStartedConsumingRequestBody = true;

        if (!RequestUpgrade && !ExtendedConnect)
        {
            if (_context.MinRequestBodyDataRate != null)
            {
                _timingEnabled = true;
                _context.TimeoutControl.StartRequestBody(_context.MinRequestBodyDataRate);
            }
        }

        return OnReadStartedAsync();
    }

    protected void TryStop()
    {
        if (_stopped)
        {
            return;
        }

        _stopped = true;

        if (!RequestUpgrade && !ExtendedConnect)
        {
            if (_timingEnabled)
            {
                if (_backpressure)
                {
                    _context.TimeoutControl.StopTimingRead();
                }

                _context.TimeoutControl.StopRequestBody();
            }
        }
    }

    protected virtual void OnReadStarting()
    {
    }

    protected virtual Task OnReadStartedAsync()
    {
        return Task.CompletedTask;
    }

    protected void AddAndCheckObservedBytes(long observedBytes)
    {
        _observedBytes += observedBytes;

        var maxRequestBodySize = _context.MaxRequestBodySize;
        if (_observedBytes > maxRequestBodySize)
        {
            OnObservedBytesExceedMaxRequestBodySize(maxRequestBodySize.Value);
        }
    }

    protected virtual void OnObservedBytesExceedMaxRequestBodySize(long maxRequestBodySize)
    {
        throw BadHttpRequestException.GetException(RequestRejectionReason.RequestBodyTooLarge, maxRequestBodySize.ToString(CultureInfo.InvariantCulture));
    }

    protected ValueTask<ReadResult> StartTimingReadAsync(ValueTask<ReadResult> readAwaitable, CancellationToken cancellationToken)
    {
        if (!readAwaitable.IsCompleted)
        {
            ValueTask<FlushResult> continueTask = TryProduceContinueAsync();
            if (!continueTask.IsCompletedSuccessfully)
            {
                return StartTimingReadAwaited(continueTask, readAwaitable, cancellationToken);
            }
            else
            {
                continueTask.GetAwaiter().GetResult();
            }

            if (_timingEnabled)
            {
                _backpressure = true;
                _context.TimeoutControl.StartTimingRead();
            }
        }

        return readAwaitable;
    }

    protected async ValueTask<ReadResult> StartTimingReadAwaited(ValueTask<FlushResult> continueTask, ValueTask<ReadResult> readAwaitable, CancellationToken cancellationToken)
    {
        await continueTask;

        if (_timingEnabled)
        {
            _backpressure = true;
            _context.TimeoutControl.StartTimingRead();
        }

        return await readAwaitable;
    }

    protected void CountBytesRead(long bytesInReadResult)
    {
        var numFirstSeenBytes = bytesInReadResult - _alreadyTimedBytes;

        if (numFirstSeenBytes > 0)
        {
            _context.TimeoutControl.BytesRead(numFirstSeenBytes);
        }
    }

    protected void StopTimingRead(long bytesInReadResult)
    {
        CountBytesRead(bytesInReadResult);

        if (_backpressure)
        {
            _backpressure = false;
            _context.TimeoutControl.StopTimingRead();
        }
    }

    protected long TrackConsumedAndExaminedBytes(ReadResult readResult, SequencePosition consumed, SequencePosition examined)
    {
        // This code path is fairly hard to understand so let's break it down with an example
        // ReadAsync returns a ReadResult of length 50.
        // Advance(25, 40). The examined length would be 40 and consumed length would be 25.
        // _totalExaminedInPreviousReadResult starts at 0. newlyExamined is 40.
        // OnDataRead is called with length 40.
        // _totalExaminedInPreviousReadResult is now 40 - 25 = 15.

        // The next call to ReadAsync returns 50 again
        // Advance(5, 5) is called
        // newlyExamined is 5 - 15, or -10.
        // Update _totalExaminedInPreviousReadResult to 10 as we consumed 5.

        // The next call to ReadAsync returns 50 again
        // _totalExaminedInPreviousReadResult is 10
        // Advance(50, 50) is called
        // newlyExamined = 50 - 10 = 40
        // _totalExaminedInPreviousReadResult is now 50
        // _totalExaminedInPreviousReadResult is finally 0 after subtracting consumedLength.

        long examinedLength, consumedLength, totalLength;

        if (consumed.Equals(examined))
        {
            examinedLength = readResult.Buffer.Slice(readResult.Buffer.Start, examined).Length;
            consumedLength = examinedLength;
        }
        else
        {
            consumedLength = readResult.Buffer.Slice(readResult.Buffer.Start, consumed).Length;
            examinedLength = consumedLength + readResult.Buffer.Slice(consumed, examined).Length;
        }

        if (examined.Equals(readResult.Buffer.End))
        {
            totalLength = examinedLength;
        }
        else
        {
            totalLength = readResult.Buffer.Length;
        }

        var newlyExaminedBytes = examinedLength - _examinedUnconsumedBytes;
        _examinedUnconsumedBytes += newlyExaminedBytes - consumedLength;
        _alreadyTimedBytes = totalLength - consumedLength;

        return newlyExaminedBytes;
    }
}