﻿using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Metrics;
using NZ.Orz.ReverseProxy.LoadBalancing;
using NZ.Orz.Sockets;

namespace NZ.Orz.ReverseProxy.L4;

public class L4ProxyMiddleware : IOrderMiddleware
{
    private readonly IConnectionFactory connectionFactory;
    private readonly IL4Router router;
    private readonly OrzLogger logger;
    private readonly LoadBalancingPolicy loadBalancing;
    private readonly SocketTransportOptions? options;
    private readonly TcpConnectionDelegate reqTcp;
    private readonly TcpConnectionDelegate respTcp;
    private readonly bool hasMiddlewareTcp;

    public L4ProxyMiddleware(IConnectionFactory connectionFactory, IL4Router router, OrzLogger logger, LoadBalancingPolicy loadBalancing, IRouteContractor contractor,
        IEnumerable<ITcpMiddleware> tcpMiddlewares)
    {
        this.connectionFactory = connectionFactory;
        this.router = router;
        this.logger = logger;
        this.loadBalancing = loadBalancing;
        this.options = contractor.GetSocketTransportOptions();
        (reqTcp, respTcp, hasMiddlewareTcp) = BuildMiddleware(tcpMiddlewares);
    }

    public int Order => 0;

    public async Task Invoke(ConnectionContext context, ConnectionDelegate next)
    {
        try
        {
            var route = await router.MatchAsync(context);
            if (route is null)
            {
                logger.NotFoundRouteL4(context.LocalEndPoint);
            }
            else
            {
                context.Route = route;
                if (context is UdpConnectionContext)
                {
                    // todo udp
                }
                else
                {
                    await TcpProxyAsync(context, route);
                }
            }
        }
        catch (Exception ex)
        {
            logger.UnexpectedException(ex.Message, ex);
        }
        finally
        {
            await next(context);
        }
    }

    private async Task TcpProxyAsync(ConnectionContext context, RouteConfig route)
    {
        logger.ProxyTcpBegin(route.RouteId);
        ConnectionContext upstream = null;
        try
        {
            upstream = await TryConnectionAsync(context, route);
            if (upstream is null)
            {
                logger.NotFoundAvailableUpstream(route.ClusterId);
            }
            else
            {
                context.SelectedDestination?.ConcurrencyCounter.Increment();
                var cts = route.CreateTimeoutTokenSource();
                var task = hasMiddlewareTcp ?
                        await Task.WhenAny(
                        context.Transport.Input.CopyToAsync(new MiddlewarePipeWriter(upstream.Transport.Output, context, reqTcp), cts.Token)
                        , upstream.Transport.Input.CopyToAsync(new MiddlewarePipeWriter(context.Transport.Output, context, respTcp), cts.Token))
                        : await Task.WhenAny(
                        context.Transport.Input.CopyToAsync(upstream.Transport.Output, cts.Token)
                        , upstream.Transport.Input.CopyToAsync(context.Transport.Output, cts.Token));
                if (task.IsCanceled)
                {
                    logger.ProxyTimeout(route.RouteId, route.Timeout);
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.ConnectUpstreamTimeout(route.RouteId);
        }
        catch (Exception ex)
        {
            logger.UnexpectedException(nameof(TcpProxyAsync), ex);
        }
        finally
        {
            context.SelectedDestination?.ConcurrencyCounter.Decrement();
            upstream?.Abort();
        }
        logger.ProxyTcpEnd(route.RouteId);
    }

    private async Task<ConnectionContext> TryConnectionAsync(ConnectionContext context, RouteConfig route)
    {
        var retryCount = route.RetryCount;
        return await DoConnectionAsync(context, route, retryCount);
    }

    private async Task<ConnectionContext> DoConnectionAsync(ConnectionContext context, RouteConfig route, int retryCount)
    {
        DestinationState selectedDestination = null;
        try
        {
            selectedDestination = context.SelectedDestination = loadBalancing.PickDestination(context, route);
            if (selectedDestination is null)
            {
                return null;
            }
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(options.ConnectionTimeout);
            var c = await connectionFactory.ConnectAsync(selectedDestination.EndPoint, cts.Token);
            selectedDestination.ReportSuccessed();
            return c;
        }
        catch (Exception ex)
        {
            selectedDestination?.ReportFailed();
            retryCount--;
            if (retryCount < 0)
            {
                throw;
            }
            return await DoConnectionAsync(context, route, retryCount);
        }
    }

    private (TcpConnectionDelegate req, TcpConnectionDelegate resp, bool hasMiddleware) BuildMiddleware(IEnumerable<ITcpMiddleware> middlewares)
    {
        var hasMiddleware = false;
        TcpConnectionDelegate request;
        TcpConnectionDelegate response;
        request = response = (context, s, t) =>
        {
            return Task.FromResult(s);
        };
        foreach (var p in middlewares.OrderBy(i => i.Order))
        {
            hasMiddleware = true;
            Func<TcpConnectionDelegate, TcpConnectionDelegate> component = (TcpConnectionDelegate next) => (c, s, t) => p.OnRequest(c, s, t, next);
            request = component(request);
            component = (TcpConnectionDelegate next) => (c, s, t) => p.OnResponse(c, s, t, next);
            response = component(response);
        }
        return (request, response, hasMiddleware);
    }
}