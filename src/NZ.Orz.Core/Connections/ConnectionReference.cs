using System.Diagnostics.CodeAnalysis;

namespace NZ.Orz.Connections;

public sealed class ConnectionReference
{
    private readonly long _id;
    private readonly WeakReference<OrzConnection> _weakReference;
    private readonly TransportConnectionManager _transportConnectionManager;

    public ConnectionReference(long id, OrzConnection connection, TransportConnectionManager transportConnectionManager)
    {
        _id = id;

        _weakReference = new WeakReference<OrzConnection>(connection);
        ConnectionId = connection.TransportConnection.ConnectionId;

        _transportConnectionManager = transportConnectionManager;
    }

    public string ConnectionId { get; }

    public bool TryGetConnection([NotNullWhen(true)] out OrzConnection? connection)
    {
        return _weakReference.TryGetTarget(out connection);
    }

    public void StopTransportTracking()
    {
        _transportConnectionManager.StopTracking(_id);
    }
}