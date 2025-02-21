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
        var builder = new RouteTableBuilder<IPEndPoint>();
        builder.Add("127.0.0.1:5000", new IPEndPoint(IPAddress.Parse("14.215.177.38"), 80), RouteType.Prefix);
        return builder.Build();
    }

    public TestProxyHandler(IConnectionFactory connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    public async Task Invoke(ConnectionContext connection, ConnectionDelegate next)
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(10));

        var upstream = await connectionFactory.ConnectAsync(await route.FirstAsync(connection.LocalEndPoint.ToString()), cts.Token);
        cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(10));
        var task1 = connection.Transport.Input.CopyToAsync(upstream.Transport.Output, cts.Token);
        var task2 = upstream.Transport.Input.CopyToAsync(connection.Transport.Output, cts.Token);
        await Task.WhenAny(task1, task2);
        upstream.Abort();
        await next(connection);
    }
}