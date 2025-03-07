using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Connections.Exceptions;
using NZ.Orz.Metrics;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace NZ.Orz.Sockets;

internal sealed class SocketConnectionListener : IConnectionListener
{
    private readonly SocketConnectionContextFactory _factory;
    private readonly OrzLogger _logger;
    private Socket? _listenSocket;
    private readonly SocketTransportOptions _options;
    private readonly GatewayProtocols protocols;

    public EndPoint EndPoint { get; private set; }

    internal SocketConnectionListener(
        EndPoint endpoint,
        GatewayProtocols protocols,
        IRouteContractor contractor,
        OrzLogger logger)
    {
        EndPoint = endpoint;
        this.protocols = protocols;
        _options = contractor.GetSocketTransportOptions();
        _logger = logger;
        _factory = new SocketConnectionContextFactory(contractor, logger);
    }

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
        EndPoint = listenSocket.LocalEndPoint;

        listenSocket.Listen(_options.Backlog);

        _listenSocket = listenSocket;
    }

    public async ValueTask<ConnectionContext?> AcceptAsync(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            try
            {
                Debug.Assert(_listenSocket != null, "Bind must be called first.");
                var acceptSocket = await _listenSocket.AcceptAsync(cancellationToken);

                // Only apply no delay to Tcp based endpoints
                if (acceptSocket.LocalEndPoint is IPEndPoint)
                {
                    acceptSocket.NoDelay = _options.NoDelay;
                }

                return _factory.Create(acceptSocket);
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

    public ValueTask UnbindAsync(CancellationToken cancellationToken = default)
    {
        _listenSocket?.Dispose();
        return default;
    }

    public ValueTask DisposeAsync()
    {
        _listenSocket?.Dispose();

        _factory.Dispose();

        return default;
    }
}