using NZ.Orz.Buffers;
using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Connections.Features;
using NZ.Orz.Infrastructure;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace NZ.Orz.ReverseProxy.L4;

public partial class L4ProxyMiddleware
{
    #region Sni

    private async Task SNIProxyAsync(ConnectionContext context)
    {
        using var c = cancellationTokenSourcePool.Rent();
        c.CancelAfter(options.ConnectionTimeout);
        var (route, r) = await router.MatchSNIAsync(context, c.Token);
        if (route is not null)
        {
            context.Route = route;
            logger.ProxyBegin(route.RouteId);
            if (route.Ssl.Passthrough)
            {
                await DoPassthroughAsync(context, route, r);
            }
            else
            {
                await DoSslAsync(context, route, r);
            }

            logger.ProxyEnd(route.RouteId);
        }
    }

    private async Task DoSslAsync(ConnectionContext context, RouteConfig route, ReadResult r)
    {
        var sslConfig = route.Ssl;
        var sslDuplexPipe = CreateSslDuplexPipe(r, context.Transport, context is IMemoryPoolFeature s ? s.MemoryPool : MemoryPool<byte>.Shared, sslConfig.SslStreamFactory);
        var sslStream = sslDuplexPipe.Stream;
        context.Transport = sslDuplexPipe;
        using var cts = cancellationTokenSourcePool.Rent();
        cts.CancelAfter(sslConfig.HandshakeTimeout);
        await sslStream.AuthenticateAsServerAsync(sslConfig.Options, cts.Token);
        await TcpProxyAsync(context, route);
    }

    private SslDuplexPipe CreateSslDuplexPipe(ReadResult readResult, IDuplexPipe transport, MemoryPool<byte> memoryPool, Func<Stream, SslStream> sslStreamFactory)
    {
        StreamPipeReaderOptions inputPipeOptions = new StreamPipeReaderOptions
        (
            pool: memoryPool,
            bufferSize: memoryPool.GetMinimumSegmentSize(),
            minimumReadSize: memoryPool.GetMinimumAllocSize(),
            leaveOpen: true,
            useZeroByteReads: true
        );

        var outputPipeOptions = new StreamPipeWriterOptions
        (
            pool: memoryPool,
            leaveOpen: true
        );

        return new SslDuplexPipe(readResult, transport, inputPipeOptions, outputPipeOptions, sslStreamFactory);
    }

    private async Task DoPassthroughAsync(ConnectionContext context, RouteConfig? route, ReadResult r)
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
                using var cts = route.CreateTimeoutTokenSource(cancellationTokenSourcePool);
                var t = cts.Token;
                await r.CopyToAsync(upstream.Transport.Output, t);
                context.Transport.Input.AdvanceTo(r.Buffer.End);
                var task = hasMiddlewareTcp ?
                        await Task.WhenAny(
                        context.Transport.Input.CopyToAsync(new MiddlewarePipeWriter(upstream.Transport.Output, context, reqTcp), t)
                        , upstream.Transport.Input.CopyToAsync(new MiddlewarePipeWriter(context.Transport.Output, context, respTcp), t))
                        : await Task.WhenAny(
                        context.Transport.Input.CopyToAsync(upstream.Transport.Output, t)
                        , upstream.Transport.Input.CopyToAsync(context.Transport.Output, t));
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

    #endregion Sni
}