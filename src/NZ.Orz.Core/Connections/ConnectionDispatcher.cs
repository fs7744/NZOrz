using Microsoft.Extensions.Logging;
using NZ.Orz.Metrics;
using NZ.Orz.Servers;

namespace NZ.Orz.Connections;

internal sealed class ConnectionDispatcher<T> where T : BaseConnectionContext
{
    private readonly ServiceContext _serviceContext;
    private readonly Func<T, Task> _connectionDelegate;
    private readonly TransportConnectionManager _transportConnectionManager;
    private readonly TaskCompletionSource _acceptLoopTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

    public ConnectionDispatcher(ServiceContext serviceContext, Func<T, Task> connectionDelegate, TransportConnectionManager transportConnectionManager)
    {
        _serviceContext = serviceContext;
        _connectionDelegate = connectionDelegate;
        _transportConnectionManager = transportConnectionManager;
    }

    private OrzTrace Log => _serviceContext.Log;
    private OrzMetrics Metrics => _serviceContext.Metrics;

    public Task StartAcceptingConnections(IConnectionListener<T> listener)
    {
        ThreadPool.UnsafeQueueUserWorkItem(StartAcceptingConnectionsCore, listener, preferLocal: false);
        return _acceptLoopTcs.Task;
    }

    private void StartAcceptingConnectionsCore(IConnectionListener<T> listener)
    {
        _ = AcceptConnectionsAsync();

        async Task AcceptConnectionsAsync()
        {
            try
            {
                while (true)
                {
                    var connection = await listener.AcceptAsync();

                    if (connection == null)
                    {
                        break;
                    }

                    var id = _transportConnectionManager.GetNewConnectionId();

                    var metricsContext = Metrics.CreateContext(connection);

                    var kestrelConnection = new OrzConnection<T>(
                        id, _serviceContext, _transportConnectionManager, _connectionDelegate, connection, Log, metricsContext);

                    _transportConnectionManager.AddConnection(id, kestrelConnection);

                    Log.ConnectionAccepted(connection.ConnectionId);
                    Metrics.ConnectionQueuedStart(metricsContext);

                    ThreadPool.UnsafeQueueUserWorkItem(kestrelConnection, preferLocal: false);
                }
            }
            catch (Exception ex)
            {
                Log.LogCritical(0, ex, "The connection listener failed to accept any new connections.");
            }
            finally
            {
                _acceptLoopTcs.TrySetResult();
            }
        }
    }
}