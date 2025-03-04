using Microsoft.Extensions.Logging;
using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.ReverseProxy.LoadBalancing;
using NZ.Orz.Sockets;

namespace NZ.Orz.ReverseProxy.L4;

public class L4ProxyMiddleware : IOrderMiddleware
{
    private readonly IConnectionFactory connectionFactory;
    private readonly IL4Router router;
    private readonly ILogger<L4ProxyMiddleware> logger;
    private readonly LoadBalancingPolicy loadBalancing;
    private readonly SocketTransportOptions? options;

    public L4ProxyMiddleware(IConnectionFactory connectionFactory, IL4Router router, ILogger<L4ProxyMiddleware> logger, LoadBalancingPolicy loadBalancing, IRouteContractor contractor)
    {
        this.connectionFactory = connectionFactory;
        this.router = router;
        this.logger = logger;
        this.loadBalancing = loadBalancing;
        this.options = contractor.GetSocketTransportOptions();
    }

    public int Order => 0;

    public async Task Invoke(ConnectionContext context, ConnectionDelegate next)
    {
        try
        {
            var route = await router.MatchAsync(context);
            if (route == null)
            {
                logger.LogWarning($"No match route {context.LocalEndPoint}");
            }
            else
            {
                context.Route = route;
                if (context is UdpConnectionContext)
                {
                    // todo
                }
                else
                {
                    var upstream = await TryConnectionAsync(context, route);
                    if (upstream == null)
                    {
                        logger.LogWarning($"No available upstream for {route.ClusterId}");
                    }
                    else
                    {
                        try
                        {
                            context.SelectedDestination?.ConcurrencyCounter.Increment();
                            var cts = route.CreateTimeoutTokenSource();
                            var task1 = context.Transport.Input.CopyToAsync(upstream.Transport.Output, cts.Token);
                            var task2 = upstream.Transport.Input.CopyToAsync(context.Transport.Output, cts.Token);
                            await Task.WhenAny(task1, task2);
                        }
                        finally
                        {
                            context.SelectedDestination?.ConcurrencyCounter.Decrement();
                            upstream.Abort();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
        finally
        {
            await next(context);
        }
    }

    private async Task<ConnectionContext> TryConnectionAsync(ConnectionContext context, RouteConfig route)
    {
        var retryCount = route.RetryCount;
        return await DoConnectionAsync(context, route, retryCount);
    }

    private async Task<ConnectionContext> DoConnectionAsync(ConnectionContext context, RouteConfig route, int retryCount)
    {
        try
        {
            var selectedDestination = context.SelectedDestination = loadBalancing.PickDestination(context, route);
            if (selectedDestination == null)
            {
                return null;
            }
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(options.ConnectionTimeout);
            return await connectionFactory.ConnectAsync(selectedDestination.EndPoint, cts.Token);
        }
        catch
        {
            retryCount--;
            if (retryCount <= 0)
            {
                throw;
            }
            return await DoConnectionAsync(context, route, retryCount);
        }
    }
}