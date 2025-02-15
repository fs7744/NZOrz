using Microsoft.Extensions.Logging;
using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Sockets.Internal;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;

namespace NZ.Orz.Sockets.Client;

public class SocketConnectionFactory : IConnectionFactory, IAsyncDisposable
{
    private readonly SocketTransportOptions _options;
    private readonly MemoryPool<byte> _memoryPool;
    private readonly ILogger _trace;
    private readonly PipeOptions _inputOptions;
    private readonly PipeOptions _outputOptions;
    private readonly SocketSenderPool _socketSenderPool;

    public SocketConnectionFactory(IRouteContractor contractor, ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(contractor);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _options = contractor.GetSocketTransportOptions();
        _memoryPool = _options.MemoryPoolFactory();
        _trace = loggerFactory.CreateLogger("NZ.Orz.Sockets.Client");

        var maxReadBufferSize = _options.MaxReadBufferSize ?? 0;
        var maxWriteBufferSize = _options.MaxWriteBufferSize ?? 0;

        var applicationScheduler = _options.UnsafePreferInlineScheduling ? PipeScheduler.Inline : PipeScheduler.ThreadPool;
        var transportScheduler = applicationScheduler;
        var awaiterScheduler = OperatingSystem.IsWindows() ? transportScheduler : PipeScheduler.Inline;

        _inputOptions = new PipeOptions(_memoryPool, applicationScheduler, transportScheduler, maxReadBufferSize, maxReadBufferSize / 2, useSynchronizationContext: false);
        _outputOptions = new PipeOptions(_memoryPool, transportScheduler, applicationScheduler, maxWriteBufferSize, maxWriteBufferSize / 2, useSynchronizationContext: false);
        _socketSenderPool = new SocketSenderPool(awaiterScheduler);
    }

    public async ValueTask<ConnectionContext> ConnectAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
    {
        var ipEndPoint = endpoint as IPEndPoint;

        if (ipEndPoint is null)
        {
            throw new NotSupportedException("The SocketConnectionFactory only supports IPEndPoints for now.");
        }

        var socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = _options.NoDelay
        };

        await socket.ConnectAsync(ipEndPoint, cancellationToken);

        var socketConnection = new SocketConnection(
            socket,
            _memoryPool,
            _inputOptions.ReaderScheduler,
            _trace,
            _socketSenderPool,
            _inputOptions,
            _outputOptions,
            _options.WaitForDataBeforeAllocatingBuffer);

        socketConnection.Start();
        return socketConnection;
    }

    public ValueTask DisposeAsync()
    {
        _memoryPool.Dispose();
        return default;
    }
}