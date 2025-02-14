using NZ.Orz.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TcpDemo;

public class TestProxyHandler : IMiddleware
{
    private readonly IConnectionFactory connectionFactory;
    private readonly IPEndPoint proxyServer = new(IPAddress.Parse("14.215.177.38"), 80);
    private ConnectionContext upstream;

    public TestProxyHandler(IConnectionFactory connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    public async Task Invoke(ConnectionContext connection, ConnectionDelegate next)
    {
        upstream = await connectionFactory.ConnectAsync(proxyServer);
        var task1 = connection.Transport.Input.CopyToAsync(upstream.Transport.Output);
        var task2 = upstream.Transport.Input.CopyToAsync(connection.Transport.Output);
        await Task.WhenAny(task1, task2);
        await next(connection);
    }
}