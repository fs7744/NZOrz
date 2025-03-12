﻿using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Metrics;
using NZ.Orz.ReverseProxy.LoadBalancing;
using NZ.Orz.Sockets;
using NZ.Orz.Sockets.Client;
using System.IO;
using System.Net.Sockets;

namespace NZ.Orz.ReverseProxy.L4;

public class L4ProxyMiddleware : IOrderMiddleware
{
    private readonly IConnectionFactory connectionFactory;
    private readonly IUdpConnectionFactory udp;
    private readonly IL4Router router;
    private readonly OrzLogger logger;
    private readonly LoadBalancingPolicy loadBalancing;
    private readonly SocketTransportOptions? options;
    private readonly TcpConnectionDelegate reqTcp;
    private readonly TcpConnectionDelegate respTcp;
    private readonly bool hasMiddlewareTcp;

    public L4ProxyMiddleware(IConnectionFactory connectionFactory, IUdpConnectionFactory udp, IL4Router router, OrzLogger logger, LoadBalancingPolicy loadBalancing, IRouteContractor contractor,
        IEnumerable<ITcpMiddleware> tcpMiddlewares)
    {
        this.connectionFactory = connectionFactory;
        this.udp = udp;
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
                logger.ProxyBegin(route.RouteId);
                if (context is UdpConnectionContext udp)
                {
                    await UdpProxyAsync(udp, route);
                }
                else
                {
                    await TcpProxyAsync(context, route);
                }
                logger.ProxyEnd(route.RouteId);
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

    private async Task UdpProxyAsync(UdpConnectionContext context, RouteConfig route)
    {
        try
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var cts = route.CreateTimeoutTokenSource();
            var token = cts.Token;
            if (await DoUdpSendToAsync(socket, context, route, route.RetryCount, token))
            {
                var c = route.UdpResponses;
                while (c > 0)
                {
                    var r = await udp.ReceiveAsync(socket, token);
                    c--;
                    await udp.SendToAsync(context.Socket, context.RemoteEndPoint, r.ReceivedBytes, token);
                }
            }
            else
            {
                logger.NotFoundAvailableUpstream(route.ClusterId);
            }
        }
        catch (OperationCanceledException)
        {
            logger.ConnectUpstreamTimeout(route.RouteId);
        }
        catch (Exception ex)
        {
            logger.UnexpectedException(nameof(UdpProxyAsync), ex);
        }
        finally
        {
            context.SelectedDestination?.ConcurrencyCounter.Decrement();
        }
    }

    private async Task<bool> DoUdpSendToAsync(Socket socket, UdpConnectionContext context, RouteConfig route, int retryCount, CancellationToken cancellationToken)
    {
        DestinationState selectedDestination = null;
        try
        {
            selectedDestination = context.SelectedDestination = loadBalancing.PickDestination(context, route);
            if (selectedDestination is null)
            {
                return false;
            }
            await udp.SendToAsync(socket, selectedDestination.EndPoint, context.ReceivedBytes, cancellationToken);
            selectedDestination.ReportSuccessed();
            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            selectedDestination?.ReportFailed();
            retryCount--;
            if (retryCount < 0)
            {
                throw;
            }
            return await DoUdpSendToAsync(socket, context, route, retryCount, cancellationToken);
        }
    }

    private async Task TcpProxyAsync(ConnectionContext context, RouteConfig route)
    {
        ConnectionContext upstream = null;
        try
        {
            upstream = await DoConnectionAsync(context, route, route.RetryCount);
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