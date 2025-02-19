using NZ.Orz.Connections.Features;
using NZ.Orz.Connections;
using NZ.Orz.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Net;
using System.Buffers;

namespace NZ.Orz.Sockets;

public sealed class UdpConnectionContext : TransportConnection
{
    private readonly IMemoryOwner<byte> memory;

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