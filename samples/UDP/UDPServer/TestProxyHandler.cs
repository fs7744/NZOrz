using NZ.Orz.Connections;
using NZ.Orz.Sockets;
using NZ.Orz.Sockets.Client;
using System.Net;
using System.Net.Sockets;

namespace UDPServer;

public class TestProxyHandler : IMiddleware
{
    private readonly IUdpConnectionFactory connectionFactory;
    private readonly IPEndPoint proxyServer = new(IPAddress.Parse("127.0.0.1"), 11000);
    private ConnectionContext upstream;

    public TestProxyHandler(IUdpConnectionFactory connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    public async Task Invoke(ConnectionContext connection, ConnectionDelegate next)
    {
        if (connection is UdpConnectionContext context)
        {
            Console.WriteLine($"{context.LocalEndPoint} received {context.ReceivedBytesCount} from {context.RemoteEndPoint}");
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            await connectionFactory.SendToAsync(socket, proxyServer, context.ReceivedBytes, CancellationToken.None);
            var r = await connectionFactory.ReceiveAsync(socket, CancellationToken.None);
            await connectionFactory.SendToAsync(context.Socket, context.RemoteEndPoint, r.ReceivedBytes, CancellationToken.None);
            //context.Abort();
        }

        await next(connection);
    }
}