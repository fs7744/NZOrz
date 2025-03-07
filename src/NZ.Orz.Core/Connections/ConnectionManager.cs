using NZ.Orz.Infrastructure;
using NZ.Orz.Metrics;
using System.Collections.Concurrent;

namespace NZ.Orz.Connections;

public sealed class ConnectionManager : IHeartbeatHandler
{
    private readonly Action<OrzConnection> _walkCallback;

    private long _lastConnectionId = long.MinValue;

    private readonly ConcurrentDictionary<long, ConnectionReference> _connectionReferences = new ConcurrentDictionary<long, ConnectionReference>();
    private readonly OrzLogger _trace;

    public ConnectionManager(OrzLogger trace, long? upgradedConnectionLimit)
        : this(trace, GetCounter(upgradedConnectionLimit))
    {
    }

    public ConnectionManager(OrzLogger trace, ResourceCounter upgradedConnections)
    {
        UpgradedConnectionCount = upgradedConnections;
        _trace = trace;
        _walkCallback = WalkCallback;
    }

    public long GetNewConnectionId() => Interlocked.Increment(ref _lastConnectionId);

    public ResourceCounter UpgradedConnectionCount { get; }

    public void OnHeartbeat()
    {
        Walk(_walkCallback);
    }

    private void WalkCallback(OrzConnection connection)
    {
        connection.TickHeartbeat();
    }

    public void AddConnection(long id, ConnectionReference connectionReference)
    {
        if (!_connectionReferences.TryAdd(id, connectionReference))
        {
            throw new ArgumentException("Unable to add connection.", nameof(id));
        }
    }

    public void RemoveConnection(long id)
    {
        if (!_connectionReferences.TryRemove(id, out var reference))
        {
            throw new ArgumentException("Unable to remove connection.", nameof(id));
        }

        if (reference.TryGetConnection(out var connection))
        {
            connection.Complete();
        }
    }

    public void Walk(Action<OrzConnection> callback)
    {
        foreach (var kvp in _connectionReferences)
        {
            var reference = kvp.Value;

            if (reference.TryGetConnection(out var connection))
            {
                callback(connection);
            }
            else if (_connectionReferences.TryRemove(kvp.Key, out reference))
            {
                _trace.ApplicationNeverCompleted(reference.ConnectionId);
                reference.StopTransportTracking();
            }
        }
    }

    private static ResourceCounter GetCounter(long? number)
        => number.HasValue
            ? ResourceCounter.Quota(number.Value)
            : ResourceCounter.Unlimited;
}