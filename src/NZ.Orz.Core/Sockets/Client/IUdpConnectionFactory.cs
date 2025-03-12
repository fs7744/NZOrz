using System.Net;
using System.Net.Sockets;

namespace NZ.Orz.Sockets.Client;

public interface IUdpConnectionFactory
{
    ValueTask<UdpConnectionContext> ReceiveAsync(Socket socket, CancellationToken cancellationToken);

    Task<int> SendToAsync(Socket socket, EndPoint remoteEndPoint, Memory<byte> receivedBytes, CancellationToken cancellationToken);
}