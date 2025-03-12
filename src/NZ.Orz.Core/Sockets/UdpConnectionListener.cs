using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Connections.Exceptions;
using NZ.Orz.Metrics;
using NZ.Orz.Sockets.Client;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace NZ.Orz.Sockets;

internal sealed class UdpConnectionListener : IConnectionListener
{
    private EndPoint? udpEndPoint;
    private readonly GatewayProtocols protocols;
    private OrzLogger _logger;
    private readonly IUdpConnectionFactory connectionFactory;
    private readonly Func<EndPoint, GatewayProtocols, Socket> createBoundListenSocket;
    private Socket? _listenSocket;

    public UdpConnectionListener(EndPoint? udpEndPoint, GatewayProtocols protocols, IRouteContractor contractor, OrzLogger logger, IUdpConnectionFactory connectionFactory)
    {
        this.udpEndPoint = udpEndPoint;
        this.protocols = protocols;
        _logger = logger;
        this.connectionFactory = connectionFactory;
        createBoundListenSocket = contractor.GetSocketTransportOptions().CreateBoundListenSocket;
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
            listenSocket = createBoundListenSocket(EndPoint, protocols);
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
                var r = await connectionFactory.ReceiveAsync(_listenSocket, cancellationToken);
                return new UdpConnectionContext(_listenSocket, r);
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
                _logger.ConnectionReset("(null)");
            }
        }
    }

    public ValueTask DisposeAsync()
    {
        _listenSocket?.Dispose();

        return default;
    }

    public ValueTask UnbindAsync(CancellationToken cancellationToken = default)
    {
        _listenSocket?.Dispose();
        return default;
    }
}