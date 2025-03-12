using System.Collections.Concurrent;
using System.IO.Pipelines;

namespace NZ.Orz.Sockets.Internal;

internal sealed class UdpSenderPool : IDisposable
{
    private const int MaxQueueSize = 1024;

    private readonly ConcurrentQueue<UdpSender> _queue = new();
    private int _count;
    private readonly PipeScheduler _scheduler;
    private bool _disposed;

    public UdpSenderPool(PipeScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public PipeScheduler Scheduler => _scheduler;

    public UdpSender Rent()
    {
        if (_queue.TryDequeue(out var sender))
        {
            Interlocked.Decrement(ref _count);
            return sender;
        }
        return new UdpSender(_scheduler);
    }

    public void Return(UdpSender sender)
    {
        // This counting isn't accurate, but it's good enough for what we need to avoid using _queue.Count which could be expensive
        if (_disposed || Interlocked.Increment(ref _count) > MaxQueueSize)
        {
            Interlocked.Decrement(ref _count);
            sender.Dispose();
            return;
        }

        sender.RemoteEndPoint = null;
        sender.SetBuffer(null, 0, 0);
        _queue.Enqueue(sender);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            while (_queue.TryDequeue(out var sender))
            {
                sender.Dispose();
            }
        }
    }
}