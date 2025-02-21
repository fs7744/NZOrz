using NZ.Orz.Connections;
using NZ.Orz.Routing;
using System.Net;

namespace TcpDemo;

public class TestProxyHandler : IMiddleware
{
    private readonly IConnectionFactory connectionFactory;
    private static RouteTable<IPEndPoint> route = CreateRouteTable();

    private static RouteTable<IPEndPoint> CreateRouteTable()
    {
        var trie = new RadixTrie<IPEndPoint>();
        return new RouteTable<IPEndPoint>(new Dictionary<string, IPEndPoint>()
        {
            { IPEndPoint.Parse("127.0.0.1:5000").ToString(),new(IPAddress.Parse("14.215.177.38"), 80)}
        }, trie);
    }

    private ConnectionContext upstream;

    public TestProxyHandler(IConnectionFactory connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    public async Task Invoke(ConnectionContext connection, ConnectionDelegate next)
    {
        upstream = await connectionFactory.ConnectAsync(await route.FindAsync(connection.LocalEndPoint.ToString()));
        var task1 = connection.Transport.Input.CopyToAsync(upstream.Transport.Output);
        var task2 = upstream.Transport.Input.CopyToAsync(connection.Transport.Output);
        await Task.WhenAny(task1, task2);
        await next(connection);
    }
}