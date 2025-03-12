using NZ.Orz.Connections.Exceptions;
using NZ.Orz.Connections.Features;
using NZ.Orz.Features;
using NZ.Orz.Infrastructure;
using System.Net;

namespace NZ.Orz.Connections;

public abstract class TransportMultiplexedConnection : MultiplexedConnectionContext, IConnectionIdFeature,
                                                 IConnectionItemsFeature,
                                                 IConnectionLifetimeFeature
{
    private string? _connectionId;

    // Will only have a value if the transport is created from a multiplexed transport.
    public IFeatureCollection? MultiplexedConnectionFeatures { get; protected set; }

    private void InitFeatures()
    {
        if (_Parameters != null)
        {
            _Parameters.SetFeature<IConnectionIdFeature>(this);
            _Parameters.SetFeature<IConnectionLifetimeFeature>(this);
            _Parameters.SetFeature<IConnectionItemsFeature>(this);
            InitializeFeatures();
        }
    }

    public override EndPoint? LocalEndPoint { get; set; }
    public override EndPoint? RemoteEndPoint { get; set; }

    public override string ConnectionId
    {
        get => _connectionId ??= CorrelationIdGenerator.GetNextId();
        set => _connectionId = value;
    }

    internal void ResetItems()
    {
        _Parameters?.Clear();
        InitFeatures();
    }

    internal virtual void InitializeFeatures()
    {
    }

    public override CancellationToken ConnectionClosed { get; set; }

    private IFeatureCollection _Parameters;

    public override IFeatureCollection Parameters
    {
        get
        {
            if (_Parameters is null)
            {
                _Parameters = new FeatureCollection();
                InitFeatures();
            }
            return _Parameters;
        }
    }

    public override void Abort(ConnectionAbortedException abortReason)
    {
        throw abortReason;
    }
}