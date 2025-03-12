using System.Net;
using System.Net.Sockets;

namespace NZ.Orz.Sockets.Client;

public interface IUdpConnectionFactory
{
    ValueTask<UdpReceiveFromResult> ReceiveAsync(Socket socket, CancellationToken cancellationToken);

    Task<int> SendToAsync(Socket socket, EndPoint remoteEndPoint, ReadOnlyMemory<byte> receivedBytes, CancellationToken cancellationToken);
}