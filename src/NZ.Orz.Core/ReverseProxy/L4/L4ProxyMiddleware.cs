using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Infrastructure;
using NZ.Orz.Metrics;
using NZ.Orz.ReverseProxy.LoadBalancing;
using NZ.Orz.Sockets;
using NZ.Orz.Sockets.Client;
using System.Net.Sockets;

namespace NZ.Orz.ReverseProxy.L4;

public class L4ProxyMiddleware : IOrderMiddleware
{
    private readonly CancellationTokenSourcePool cancellationTokenSourcePool = new();
    private readonly IConnectionFactory connectionFactory;
    private readonly IUdpConnectionFactory udp;
    private readonly IL4Router router;
    private readonly OrzLogger logger;
    private readonly LoadBalancingPolicy loadBalancing;
    private readonly SocketTransportOptions? options;
    private readonly ProxyConnectionDelegate reqTcp;
    private readonly ProxyConnectionDelegate respTcp;
    private readonly bool hasMiddlewareTcp;
    private readonly ProxyConnectionDelegate reqUdp;
    private readonly ProxyConnectionDelegate respUdp;
    private readonly bool hasMiddlewareUdp;

    public L4ProxyMiddleware(IConnectionFactory connectionFactory, IUdpConnectionFactory udp, IL4Router router, OrzLogger logger, LoadBalancingPolicy loadBalancing, IRouteContractor contractor,
        IEnumerable<ITcpMiddleware> tcpMiddlewares, IEnumerable<IUdpMiddleware> udpMiddlewares)
    {
        this.connectionFactory = connectionFactory;
        this.udp = udp;
        this.router = router;
        this.logger = logger;
        this.loadBalancing = loadBalancing;
        this.options = contractor.GetSocketTransportOptions();
        (reqTcp, respTcp, hasMiddlewareTcp) = BuildMiddleware(tcpMiddlewares);
        (reqUdp, respUdp, hasMiddlewareUdp) = BuildMiddleware(udpMiddlewares);
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
                switch (context.Protocols)
                {
                    case GatewayProtocols.TCP:
                        await TcpProxyAsync(context, route);
                        break;

                    case GatewayProtocols.UDP:
                        await UdpProxyAsync((UdpConnectionContext)context, route);
                        break;

                    case GatewayProtocols.SNI:
                        await SNIProxyAsync(context, route);
                        break;
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

    #region Sni

    private async Task SNIProxyAsync(ConnectionContext context, RouteConfig route)
    {
        throw new NotImplementedException();
    }

    #endregion Sni

    #region Udp

    private async Task UdpProxyAsync(UdpConnectionContext context, RouteConfig route)
    {
        try
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var cts = route.CreateTimeoutTokenSource(cancellationTokenSourcePool);
            var token = cts.Token;
            if (await DoUdpSendToAsync(socket, context, route, route.RetryCount, await reqUdp(context, context.ReceivedBytes, token), token))
            {
                var c = route.UdpResponses;
                while (c > 0)
                {
                    var r = await udp.ReceiveAsync(socket, token);
                    c--;
                    await udp.SendToAsync(context.Socket, context.RemoteEndPoint, await respUdp(context, r.GetReceivedBytes(), token), token);
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

    private async Task<bool> DoUdpSendToAsync(Socket socket, UdpConnectionContext context, RouteConfig route, int retryCount, ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken)
    {
        DestinationState selectedDestination = null;
        try
        {
            selectedDestination = context.SelectedDestination = loadBalancing.PickDestination(context, route);
            if (selectedDestination is null)
            {
                return false;
            }
            await udp.SendToAsync(socket, selectedDestination.EndPoint, bytes, cancellationToken);
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
            return await DoUdpSendToAsync(socket, context, route, retryCount, bytes, cancellationToken);
        }
    }

    #endregion Udp

    #region Tcp

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
                var cts = route.CreateTimeoutTokenSource(cancellationTokenSourcePool);
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
            var cts = cancellationTokenSourcePool.Rent();
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

    #endregion Tcp

    private (ProxyConnectionDelegate req, ProxyConnectionDelegate resp, bool hasMiddleware) BuildMiddleware(IEnumerable<IProxyMiddleware> middlewares)
    {
        var hasMiddleware = false;
        ProxyConnectionDelegate request;
        ProxyConnectionDelegate response;
        request = response = (context, s, t) =>
        {
            return Task.FromResult(s);
        };
        foreach (var p in middlewares.OrderBy(i => i.Order))
        {
            hasMiddleware = true;
            Func<ProxyConnectionDelegate, ProxyConnectionDelegate> component = (ProxyConnectionDelegate next) => (c, s, t) => p.OnRequest(c, s, t, next);
            request = component(request);
            component = (ProxyConnectionDelegate next) => (c, s, t) => p.OnResponse(c, s, t, next);
            response = component(response);
        }
        return (request, response, hasMiddleware);
    }
}