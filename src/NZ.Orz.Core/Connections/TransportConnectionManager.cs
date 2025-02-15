using NZ.Orz.Connections.Exceptions;
using NZ.Orz.Connections.Features;
using NZ.Orz.Metrics;
using System.Collections.Concurrent;

namespace NZ.Orz.Connections;

public sealed class TransportConnectionManager
{
    private readonly ConnectionManager _connectionManager;
    private readonly ConcurrentDictionary<long, ConnectionReference> _connectionReferences = new ConcurrentDictionary<long, ConnectionReference>();

    public TransportConnectionManager(ConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public long GetNewConnectionId() => _connectionManager.GetNewConnectionId();

    public void AddConnection(long id, OrzConnection connection)
    {
        var connectionReference = new ConnectionReference(id, connection, this);

        if (!_connectionReferences.TryAdd(id, connectionReference))
        {
            throw new ArgumentException("Unable to add specified id.", nameof(id));
        }

        _connectionManager.AddConnection(id, connectionReference);
    }

    public void RemoveConnection(long id)
    {
        if (!_connectionReferences.TryRemove(id, out _))
        {
            throw new ArgumentException("No value found for the specified id.", nameof(id));
        }

        _connectionManager.RemoveConnection(id);
    }

    public void StopTracking(long id)
    {
        if (!_connectionReferences.TryRemove(id, out _))
        {
            throw new ArgumentException("No value found for the specified id.", nameof(id));
        }
    }

    public async Task<bool> CloseAllConnectionsAsync(CancellationToken token)
    {
        var closeTasks = new List<Task>();

        foreach (var kvp in _connectionReferences)
        {
            if (kvp.Value.TryGetConnection(out var connection))
            {
                connection.RequestClose();
                closeTasks.Add(connection.ExecutionTask);
            }
        }

        var allClosedTask = Task.WhenAll([.. closeTasks]);
        return await Task.WhenAny(allClosedTask, CancellationTokenAsTask(token)).ConfigureAwait(false) == allClosedTask;
    }

    public async Task<bool> AbortAllConnectionsAsync()
    {
        var abortTasks = new List<Task>();

        foreach (var kvp in _connectionReferences)
        {
            if (kvp.Value.TryGetConnection(out var connection))
            {
                OrzMetrics.AddConnectionEndReason(
                    connection.TransportConnection.Parameters.GetFeature<IConnectionMetricsContextFeature>()?.MetricsContext,
                    ConnectionEndReason.AppShutdownTimeout, overwrite: true);

                connection.TransportConnection.Abort(new ConnectionAbortedException("The connection was aborted because the server is shutting down and request processing didn't complete within the time specified by HostOptions.ShutdownTimeout."));
                abortTasks.Add(connection.ExecutionTask);
            }
        }

        var allAbortedTask = Task.WhenAll([.. abortTasks]);
        return await Task.WhenAny(allAbortedTask, Task.Delay(1000)).ConfigureAwait(false) == allAbortedTask;
    }

    private static Task CancellationTokenAsTask(CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        token.Register(tcs.SetResult);
        return tcs.Task;
    }
}