using Microsoft.Extensions.Logging;
using NZ.Orz.Connections;
using NZ.Orz.Routing;
using System.Net;

namespace TcpDemo;

public class TestProxyHandler : IMiddleware
{
    private readonly IConnectionFactory connectionFactory;
    private readonly ILogger<TestProxyHandler> logger;
    private static RouteTable<TcpRouteData> route = CreateRouteTable();

    private static RouteTable<TcpRouteData> CreateRouteTable()
    {
        var builder = new RouteTableBuilder<TcpRouteData>();
        builder.Add("127.0.0.1:5000", new TcpRouteData() { Backends = new List<TcpRouteBackendData>() { new TcpRouteBackendData() { Address = "14.215.177.38", Port = 80 } } }, RouteType.Prefix);
        builder.Add("[::1]:500", new TcpRouteData() { Backends = new List<TcpRouteBackendData>() { new TcpRouteBackendData() { Address = "14.215.177.38", Port = 80 } }, ConnectTimeout = 1, ReadTimeout = 10, WriteTimeout = 10 }, RouteType.Prefix);
        //builder.Add("127.0.0.1:5000", new IPEndPoint(IPAddress.Parse("14.215.177.31"), 80), RouteType.Prefix);
        return builder.Build();
    }

    public TestProxyHandler(IConnectionFactory connectionFactory, ILogger<TestProxyHandler> logger)
    {
        this.connectionFactory = connectionFactory;
        this.logger = logger;
    }

    public async Task Invoke(ConnectionContext connection, ConnectionDelegate next)
    {
        var r = await route.MatchAsync(connection.LocalEndPoint.ToString(), connection, MatchRoute);
        var n = r?.Backends.First();
        if (n is null)
        {
            logger.LogWarning($"No match upstream {connection.LocalEndPoint}");
            connection.Abort();
        }
        else
        {
            logger.LogWarning($"{connection.ConnectionId} match upstream {n.Address}:{n.Port}");
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(r.ConnectTimeout));
            var upstream = await connectionFactory.ConnectAsync(new IPEndPoint(IPAddress.Parse(n.Address), n.Port), cts.Token);
            cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(r.ReadTimeout));
            var task1 = connection.Transport.Input.CopyToAsync(upstream.Transport.Output, cts.Token);
            cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(r.WriteTimeout));
            var task2 = upstream.Transport.Input.CopyToAsync(connection.Transport.Output, cts.Token);
            await Task.WhenAny(task1, task2);
            upstream.Abort();
            connection.Abort();
        }
    }

    private bool MatchRoute(TcpRouteData point, ConnectionContext context)
    {
        return true;
    }
}

public class TcpRouteData
{
    public List<TcpRouteBackendData> Backends { get; set; }

    public string LoadBalancingPolicy { get; set; }
    public int ConnectTimeout { get; set; }
    public int ReadTimeout { get; set; }
    public int WriteTimeout { get; set; }
}

public class TcpRouteBackendData
{
    public string Address { get; set; }
    public int Port { get; set; }
    public decimal? Weight { get; set; }
}