using NZ.Orz.Config;
using NZ.Orz.Connections.Exceptions;
using NZ.Orz.Features;
using System.Net;

namespace NZ.Orz.Connections;

public abstract class BaseConnectionContext : IAsyncDisposable
{
    public abstract string ConnectionId { get; set; }

    public abstract IFeatureCollection Parameters { get; }

    public virtual CancellationToken ConnectionClosed { get; set; }

    public virtual EndPoint? LocalEndPoint { get; set; }

    /// <summary>
    /// to make LocalEndPoint.ToString().Reverse() only once, routeconfig may change or matchs may has not only one, so no keep route config
    /// </summary>
    public string LocalEndPointString { get; set; }

    public virtual EndPoint? RemoteEndPoint { get; set; }

    public GatewayProtocols Protocols { get; set; }

    public RouteConfig? Route { get; set; }
    public DestinationState? SelectedDestination { get; set; }

    public abstract void Abort();

    public abstract void Abort(ConnectionAbortedException abortReason);

    public virtual ValueTask DisposeAsync()
    {
        return default;
    }
}