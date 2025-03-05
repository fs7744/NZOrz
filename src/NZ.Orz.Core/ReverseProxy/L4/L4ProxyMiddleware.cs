using Microsoft.Extensions.Logging;
using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.ReverseProxy.LoadBalancing;
using NZ.Orz.Sockets;
using System.IO.Pipelines;

namespace NZ.Orz.ReverseProxy.L4;

public class MiddlewarePipeWriter : PipeWriter
{
    private readonly PipeWriter pipeWriter;
    private readonly ConnectionContext context;
    private readonly TcpConnectionDelegate connectionDelegate;

    public MiddlewarePipeWriter(PipeWriter pipeWriter, ConnectionContext context, TcpConnectionDelegate connectionDelegate)
    {
        this.pipeWriter = pipeWriter;
        this.context = context;
        this.connectionDelegate = connectionDelegate;
    }

    public override void Advance(int bytes)
    {
        pipeWriter.Advance(bytes);
    }

    public override void CancelPendingFlush()
    {
        pipeWriter.CancelPendingFlush();
    }

    public override void Complete(Exception? exception = null)
    {
        pipeWriter.Complete(exception);
    }

    public override ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken = default)
    {
        return pipeWriter.FlushAsync(cancellationToken);
    }

    public override Memory<byte> GetMemory(int sizeHint = 0)
    {
        return pipeWriter.GetMemory(sizeHint);
    }

    public override Span<byte> GetSpan(int sizeHint = 0)
    {
        return pipeWriter.GetSpan(sizeHint);
    }

    public override Stream AsStream(bool leaveOpen = false)
    {
        return pipeWriter.AsStream(leaveOpen);
    }

    public override bool CanGetUnflushedBytes => pipeWriter.CanGetUnflushedBytes;

    public override ValueTask CompleteAsync(Exception? exception = null)
    {
        return pipeWriter.CompleteAsync(exception);
    }

    public override void OnReaderCompleted(Action<Exception?, object?> callback, object? state)
    {
        pipeWriter.OnReaderCompleted(callback, state);
    }

    public override long UnflushedBytes => pipeWriter.UnflushedBytes;

    public override async ValueTask<FlushResult> WriteAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default)
    {
        return await pipeWriter.WriteAsync(await connectionDelegate(context, source, cancellationToken), cancellationToken);
    }
}

public class L4ProxyMiddleware : IOrderMiddleware
{
    private readonly IConnectionFactory connectionFactory;
    private readonly IL4Router router;
    private readonly ILogger<L4ProxyMiddleware> logger;
    private readonly LoadBalancingPolicy loadBalancing;
    private readonly SocketTransportOptions? options;
    private readonly TcpConnectionDelegate reqTcp;
    private readonly TcpConnectionDelegate respTcp;
    private readonly bool hasMiddlewareTcp;

    public L4ProxyMiddleware(IConnectionFactory connectionFactory, IL4Router router, ILogger<L4ProxyMiddleware> logger, LoadBalancingPolicy loadBalancing, IRouteContractor contractor,
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
                            if (hasMiddlewareTcp)
                            {
                                var task1 = context.Transport.Input.CopyToAsync(new MiddlewarePipeWriter(upstream.Transport.Output, context, reqTcp), cts.Token);
                                var task2 = upstream.Transport.Input.CopyToAsync(new MiddlewarePipeWriter(context.Transport.Output, context, respTcp), cts.Token);
                                await Task.WhenAny(task1, task2);
                            }
                            else
                            {
                                var task1 = context.Transport.Input.CopyToAsync(upstream.Transport.Output, cts.Token);
                                var task2 = upstream.Transport.Input.CopyToAsync(context.Transport.Output, cts.Token);
                                await Task.WhenAny(task1, task2);
                            }
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
        DestinationState selectedDestination = null;
        try
        {
            selectedDestination = context.SelectedDestination = loadBalancing.PickDestination(context, route);
            if (selectedDestination == null)
            {
                return null;
            }
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(options.ConnectionTimeout);
            var c = await connectionFactory.ConnectAsync(selectedDestination.EndPoint, cts.Token);
            selectedDestination.ReportSuccessed();
            return c;
        }
        catch
        {
            selectedDestination?.ReportFailed();
            retryCount--;
            if (retryCount <= 0)
            {
                throw;
            }
            return await DoConnectionAsync(context, route, retryCount);
        }
    }

    private (TcpConnectionDelegate req, TcpConnectionDelegate resp, bool hasMiddleware) BuildMiddleware(IEnumerable<ITcpMiddleware> middlewares)
    {
        var hasMiddleware = false;
        TcpConnectionDelegate request = (context, s, t) =>
        {
            return Task.FromResult(s);
        };
        foreach (var component in middlewares.OrderBy(i => i.Order)
            .Select<ITcpMiddleware, Func<TcpConnectionDelegate, TcpConnectionDelegate>>(p => (TcpConnectionDelegate next) => (c, s, t) => p.OnRequest(c, s, t, next)))
        {
            hasMiddleware = true;
            request = component(request);
        }

        TcpConnectionDelegate response = (context, s, t) =>
        {
            return Task.FromResult(s);
        };
        foreach (var component in middlewares.OrderBy(i => i.Order)
            .Select<ITcpMiddleware, Func<TcpConnectionDelegate, TcpConnectionDelegate>>(p => (TcpConnectionDelegate next) => (c, s, t) => p.OnResponse(c, s, t, next)))
        {
            response = component(response);
        }
        return (request, response, hasMiddleware);
    }
}