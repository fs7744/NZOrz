using NZ.Orz.Connections;
using System.Buffers;
using System.Net;
using System.Net.Sockets;

namespace NZ.Orz.Sockets;

public sealed class UdpConnectionContext : TransportConnection
{
    private readonly IMemoryOwner<byte> memory;
    public override string TransportType => "udp";
    public Socket Socket { get; }
    public int ReceivedBytesCount { get; }

    public Memory<byte> ReceivedBytes => memory.Memory.Slice(0, ReceivedBytesCount);

    public UdpConnectionContext(Socket socket, EndPoint remoteEndPoint, int receivedBytes, IMemoryOwner<byte> memory)
    {
        Socket = socket;
        ReceivedBytesCount = receivedBytes;
        this.memory = memory;
        LocalEndPoint = socket.LocalEndPoint;
        RemoteEndPoint = remoteEndPoint;
    }

    public override ValueTask DisposeAsync()
    {
        memory.Dispose();
        return base.DisposeAsync();
    }
}