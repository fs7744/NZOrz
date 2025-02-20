using Microsoft.Extensions.Logging;
using NZ.Orz.Buffers;
using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Connections.Exceptions;
using NZ.Orz.Sockets.Internal;
using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace NZ.Orz.Sockets;

internal sealed class UdpConnectionListener : IConnectionListener
{
    private EndPoint? udpEndPoint;
    private readonly GatewayProtocols protocols;
    private ILogger _logger;
    private SocketTransportOptions? _options;
    private MemoryPool<byte> udpBufferPool;

    //private readonly UdpAwaitableEventArgs _receiver;
    private Socket? _listenSocket;

    public UdpConnectionListener(EndPoint? udpEndPoint, GatewayProtocols protocols, IRouteContractor contractor, ILoggerFactory loggerFactory)
    {
        this.udpEndPoint = udpEndPoint;
        this.protocols = protocols;
        _logger = loggerFactory.CreateLogger("Orz.Server.Transport.Sockets.Udp");
        _options = contractor.GetSocketTransportOptions();
        udpBufferPool = PinnedBlockMemoryPoolFactory.Create(_options.UdpMaxSize);

        //_receiver = new UdpAwaitableEventArgs(_options.UnsafePreferInlineScheduling ? PipeScheduler.Inline : PipeScheduler.ThreadPool);
    }

    public EndPoint EndPoint => udpEndPoint;

    internal void Bind()
    {
        if (_listenSocket != null)
        {
            throw new InvalidOperationException("Transport is already bound.");
        }

        Socket listenSocket;
        try
        {
            listenSocket = _options.CreateBoundListenSocket(EndPoint, protocols);
        }
        catch (SocketException e) when (e.SocketErrorCode == SocketError.AddressAlreadyInUse)
        {
            throw new AddressInUseException(e.Message, e);
        }

        Debug.Assert(listenSocket.LocalEndPoint != null);

        _listenSocket = listenSocket;
    }

    public async ValueTask<ConnectionContext?> AcceptAsync(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            try
            {
                Debug.Assert(_listenSocket != null, "Bind must be called first.");
                var buffer = udpBufferPool.Rent();
                //var r = await _receiver.ReceiveFromAsync(_listenSocket, buffer.Memory);
                var r = await _listenSocket.ReceiveFromAsync(buffer.Memory, EndPoint, cancellationToken);
                return new UdpConnectionContext(_listenSocket, r.RemoteEndPoint, r.ReceivedBytes, buffer);
            }
            catch (ObjectDisposedException)
            {
                // A call was made to UnbindAsync/DisposeAsync just return null which signals we're done
                return null;
            }
            catch (SocketException e) when (e.SocketErrorCode == SocketError.OperationAborted)
            {
                // A call was made to UnbindAsync/DisposeAsync just return null which signals we're done
                return null;
            }
            catch (SocketException)
            {
                // The connection got reset while it was in the backlog, so we try again.
                SocketsLog.ConnectionReset(_logger, connectionId: "(null)");
            }
        }
    }

    public ValueTask DisposeAsync()
    {
        _listenSocket?.Dispose();

        //_factory.Dispose();

        return default;
    }

    public ValueTask UnbindAsync(CancellationToken cancellationToken = default)
    {
        _listenSocket?.Dispose();
        return default;
    }
}