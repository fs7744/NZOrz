using NZ.Orz.Buffers;
using NZ.Orz.Config;
using NZ.Orz.Sockets.Internal;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;

namespace NZ.Orz.Sockets.Client;

public class UdpConnectionFactory : IUdpConnectionFactory
{
    private readonly SocketTransportOptions? options;
    private readonly MemoryPool<byte> pool;
    private readonly PipeScheduler pipeScheduler;
    private readonly UdpSenderPool socketSenderPool;

    public UdpConnectionFactory(IRouteContractor contractor)
    {
        options = contractor.GetSocketTransportOptions();
        pool = PinnedBlockMemoryPoolFactory.Create(options.UdpMaxSize);

        pipeScheduler = options.UnsafePreferInlineScheduling ? PipeScheduler.Inline : PipeScheduler.ThreadPool;
        socketSenderPool = new UdpSenderPool(OperatingSystem.IsWindows() ? pipeScheduler : PipeScheduler.Inline);
    }

    public async ValueTask<UdpReceiveFromResult> ReceiveAsync(Socket socket, CancellationToken cancellationToken)
    {
        var buffer = pool.Rent();
        var receiver = new UdpAwaitableEventArgs(pipeScheduler);
        receiver.RemoteEndPoint = socket.LocalEndPoint;
        var r = await receiver.ReceiveFromAsync(socket, buffer.Memory);
        return new UdpReceiveFromResult{ RemoteEndPoint = r.RemoteEndPoint, ReceivedBytesCount = r.ReceivedBytes, Buffer = buffer };
    }

    public async Task<int> SendToAsync(Socket socket, EndPoint remoteEndPoint, ReadOnlyMemory<byte> receivedBytes, CancellationToken cancellationToken)
    {
        var sender = socketSenderPool.Rent();
        sender.RemoteEndPoint = remoteEndPoint;
        sender.SocketFlags = SocketFlags.None;
        try
        {
            return await sender.SendToAsync(socket, receivedBytes);
        }
        finally
        {
            socketSenderPool.Return(sender);
        }
    }
}