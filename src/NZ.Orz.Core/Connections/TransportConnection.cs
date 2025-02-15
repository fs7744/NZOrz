using NZ.Orz.Connections.Exceptions;
using NZ.Orz.Connections.Features;
using NZ.Orz.Features;
using NZ.Orz.Infrastructure;
using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net;

namespace NZ.Orz.Connections;

internal class TransportConnection : ConnectionContext,
                                                 IConnectionIdFeature,
                                                 IConnectionTransportFeature,
                                                 IConnectionItemsFeature,
                                                 IMemoryPoolFeature,
                                                 IConnectionLifetimeFeature
{
    private IFeatureCollection? _items;
    private string? _connectionId;

    // Will only have a value if the transport is created from a multiplexed transport.
    public IFeatureCollection? MultiplexedConnectionFeatures { get; protected set; }

    public TransportConnection()
    {
        _items = new FeatureCollection();
        InitFeatures();
    }

    private void InitFeatures()
    {
        if (_items != null)
        {
            _items.SetFeature<IConnectionIdFeature>(this);
            _items.SetFeature<IConnectionLifetimeFeature>(this);
            _items.SetFeature<IMemoryPoolFeature>(this);
            _items.SetFeature<IConnectionTransportFeature>(this);
            _items.SetFeature<IConnectionItemsFeature>(this);
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

    public virtual MemoryPool<byte> MemoryPool { get; } = default!;

    public override IDuplexPipe Transport { get; set; } = default!;

    public IDuplexPipe Application { get; set; } = default!;

    internal void ResetItems()
    {
        _items?.Clear();
        InitFeatures();
    }

    internal virtual void InitializeFeatures()
    {
    }

    public override CancellationToken ConnectionClosed { get; set; }

    public override IFeatureCollection Parameters => _items;

    // DO NOT remove this override to ConnectionContext.Abort. Doing so would cause
    // any TransportConnection that does not override Abort or calls base.Abort
    // to stack overflow when IConnectionLifetimeFeature.Abort() is called.
    // That said, all derived types should override this method should override
    // this implementation of Abort because canceling pending output reads is not
    // sufficient to abort the connection if there is backpressure.
    public override void Abort(ConnectionAbortedException abortReason)
    {
        Debug.Assert(Application != null);
        Application.Input.CancelPendingRead();
    }
}