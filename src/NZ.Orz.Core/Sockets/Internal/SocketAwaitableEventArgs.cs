﻿using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks.Sources;

namespace NZ.Orz.Sockets.Internal;

// A slimmed down version of https://github.com/dotnet/runtime/blob/82ca681cbac89d813a3ce397e0c665e6c051ed67/src/libraries/System.Net.Sockets/src/System/Net/Sockets/Socket.Tasks.cs#L798 that
// 1. Doesn't support any custom scheduling other than the PipeScheduler (no sync context, no task scheduler)
// 2. Doesn't do ValueTask validation using the token
// 3. Doesn't support usage outside of async/await (doesn't try to capture and restore the execution context)
// 4. Doesn't use cancellation tokens
internal class SocketAwaitableEventArgs : SocketAsyncEventArgs, IValueTaskSource<SocketOperationResult>
{
    private static readonly Action<object?> _continuationCompleted = _ => { };

    private readonly PipeScheduler _ioScheduler;

    // There are places where we read the _continuation field and then read some other state which we assume to be consistent
    // with the value we read in _continuation. Without a fence, those secondary reads could be reordered with respect to the first.
    // https://github.com/dotnet/runtime/pull/84432
    // https://github.com/dotnet/aspnetcore/issues/50623
    private volatile Action<object?>? _continuation;

    public SocketAwaitableEventArgs(PipeScheduler ioScheduler)
        : base(unsafeSuppressExecutionContextFlow: true)
    {
        _ioScheduler = ioScheduler;
    }

    protected override void OnCompleted(SocketAsyncEventArgs _)
    {
        var c = _continuation;

        if (c != null || (c = Interlocked.CompareExchange(ref _continuation, _continuationCompleted, null)) != null)
        {
            var continuationState = UserToken;
            UserToken = null;
            _continuation = _continuationCompleted; // in case someone's polling IsCompleted

            _ioScheduler.Schedule(c, continuationState);
        }
    }

    public SocketOperationResult GetResult(short token)
    {
        _continuation = null;

        if (SocketError != SocketError.Success)
        {
            return new SocketOperationResult(CreateException(SocketError));
        }

        return new SocketOperationResult(BytesTransferred);
    }

    protected static SocketException CreateException(SocketError e)
    {
        return new SocketException((int)e);
    }

    public ValueTaskSourceStatus GetStatus(short token)
    {
        return !ReferenceEquals(_continuation, _continuationCompleted) ? ValueTaskSourceStatus.Pending :
                SocketError == SocketError.Success ? ValueTaskSourceStatus.Succeeded :
                ValueTaskSourceStatus.Faulted;
    }

    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
    {
        UserToken = state;
        var prevContinuation = Interlocked.CompareExchange(ref _continuation, continuation, null);
        if (ReferenceEquals(prevContinuation, _continuationCompleted))
        {
            UserToken = null;
            ThreadPool.UnsafeQueueUserWorkItem(continuation, state, preferLocal: true);
        }
    }
}

//internal class UdpAwaitableEventArgs : SocketAsyncEventArgs, IValueTaskSource<SocketReceiveFromResult>
//{
//    private static readonly Action<object?> _continuationCompleted = _ => { };

//    private readonly PipeScheduler _ioScheduler;
//    private volatile Action<object?>? _continuation;

//    public UdpAwaitableEventArgs(PipeScheduler ioScheduler)
//        : base(unsafeSuppressExecutionContextFlow: true)
//    {
//        _ioScheduler = ioScheduler;
//    }

//    protected override void OnCompleted(SocketAsyncEventArgs _)
//    {
//        var c = _continuation;

//        if (c != null || (c = Interlocked.CompareExchange(ref _continuation, _continuationCompleted, null)) != null)
//        {
//            var continuationState = UserToken;
//            UserToken = null;
//            _continuation = _continuationCompleted; // in case someone's polling IsCompleted

//            _ioScheduler.Schedule(c, continuationState);
//        }
//    }

//    public SocketReceiveFromResult GetResult(short token)
//    {
//        _continuation = null;

//        SocketError error = SocketError;
//        int bytes = BytesTransferred;
//        EndPoint remoteEndPoint = RemoteEndPoint!;

//        if (error != SocketError.Success)
//        {
//            throw new SocketException((int)error);
//        }

//        return new SocketReceiveFromResult() { ReceivedBytes = bytes, RemoteEndPoint = remoteEndPoint };
//    }

//    public ValueTaskSourceStatus GetStatus(short token)
//    {
//        return !ReferenceEquals(_continuation, _continuationCompleted) ? ValueTaskSourceStatus.Pending :
//                 SocketError == SocketError.Success ? ValueTaskSourceStatus.Succeeded :
//                 ValueTaskSourceStatus.Faulted;
//    }

//    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
//    {
//        UserToken = state;
//        var prevContinuation = Interlocked.CompareExchange(ref _continuation, continuation, null);
//        if (ReferenceEquals(prevContinuation, _continuationCompleted))
//        {
//            UserToken = null;
//            ThreadPool.UnsafeQueueUserWorkItem(continuation, state, preferLocal: true);
//        }
//    }

//    public ValueTask<SocketReceiveFromResult> ReceiveFromAsync(Socket socket, Memory<byte> buffer)
//    {
//        SetBuffer(buffer);
//        Debug.Assert(Volatile.Read(ref _continuation) == null, "Expected null continuation to indicate reserved for use");

//        if (socket.ReceiveFromAsync(this))
//        {
//            return new ValueTask<SocketReceiveFromResult>(this, 0);
//        }

//        int bytesTransferred = BytesTransferred;
//        EndPoint remoteEndPoint = RemoteEndPoint!;
//        SocketError error = SocketError;

//        return error == SocketError.Success ?
//            new ValueTask<SocketReceiveFromResult>(new SocketReceiveFromResult() { ReceivedBytes = bytesTransferred, RemoteEndPoint = remoteEndPoint }) :
//            ValueTask.FromException<SocketReceiveFromResult>(CreateException(error));
//    }

//    protected static SocketException CreateException(SocketError e)
//    {
//        return new SocketException((int)e);
//    }
//}