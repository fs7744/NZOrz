using NZ.Orz.Connections;
using NZ.Orz.Sockets;
using System.Net;
using System.Text;

namespace UDPServer;

public class TestProxyHandler : IMiddleware
{
    private readonly IConnectionFactory connectionFactory;
    private readonly IPEndPoint proxyServer = new(IPAddress.Parse("127.0.0.1"), 11000);
    private ConnectionContext upstream;

    public TestProxyHandler(IConnectionFactory connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    public async Task Invoke(ConnectionContext connection, ConnectionDelegate next)
    {
        if (connection is UdpConnectionContext context)
        {
            Console.WriteLine($"{context.LocalEndPoint} received {Encoding.UTF8.GetString(context.ReceivedBytes.Span)} from {context.RemoteEndPoint}");
        }

        //upstream = await connectionFactory.ConnectAsync(proxyServer);
        //var task1 = connection.Transport.Input.CopyToAsync(upstream.Transport.Output);
        //var task2 = upstream.Transport.Input.CopyToAsync(connection.Transport.Output);
        //await Task.WhenAny(task1, task2);
        await next(connection);
    }
}