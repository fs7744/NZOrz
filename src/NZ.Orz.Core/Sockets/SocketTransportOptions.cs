using NZ.Orz.Buffers;
using System.Buffers;
using System.Net;
using System.Net.Sockets;

namespace NZ.Orz.Sockets;

public class SocketTransportOptions
{
    public bool NoDelay { get; set; } = true;

    public int Backlog { get; set; } = 512;

    public bool FinOnError { get; set; }

    public int IOQueueCount { get; set; } = Internal.IOQueue.DefaultCount;

    public bool WaitForDataBeforeAllocatingBuffer { get; set; } = true;

    public long? MaxReadBufferSize { get; set; } = 1024 * 1024;

    public long? MaxWriteBufferSize { get; set; } = 64 * 1024;

    public bool UnsafePreferInlineScheduling { get; set; }

    internal Func<MemoryPool<byte>> MemoryPoolFactory { get; set; } = PinnedBlockMemoryPoolFactory.Create;

    public Func<EndPoint, Socket> CreateBoundListenSocket { get; set; } = CreateDefaultBoundListenSocket;

    public static Socket CreateDefaultBoundListenSocket(EndPoint endpoint)
    {
        Socket listenSocket;
        switch (endpoint)
        {
            case UnixDomainSocketEndPoint unix:
                listenSocket = new Socket(unix.AddressFamily, SocketType.Stream, ProtocolType.Unspecified);
                break;

            case IPEndPoint ip:
                listenSocket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Kestrel expects IPv6Any to bind to both IPv6 and IPv4
                if (ip.Address.Equals(IPAddress.IPv6Any))
                {
                    listenSocket.DualMode = true;
                }

                break;

            default:
                listenSocket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                break;
        }

        listenSocket.Bind(endpoint);

        return listenSocket;
    }
}