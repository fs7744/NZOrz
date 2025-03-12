using NZ.Orz.Connections.Exceptions;
using NZ.Orz.Connections.Features;
using NZ.Orz.Features;
using NZ.Orz.Infrastructure;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;

namespace NZ.Orz.Connections;

public abstract class TransportConnection : ConnectionContext,
                                                 IConnectionIdFeature,
                                                 IConnectionTransportFeature,
                                                 IConnectionItemsFeature,
                                                 IMemoryPoolFeature,
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
            _Parameters.SetFeature<IMemoryPoolFeature>(this);
            _Parameters.SetFeature<IConnectionTransportFeature>(this);
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

    public virtual MemoryPool<byte> MemoryPool { get; } = default!;

    public override IDuplexPipe Transport { get; set; } = default!;

    public IDuplexPipe Application { get; set; } = default!;

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

    // DO NOT remove this override to ConnectionContext.Abort. Doing so would cause
    // any TransportConnection that does not override Abort or calls base.Abort
    // to stack overflow when IConnectionLifetimeFeature.Abort() is called.
    // That said, all derived types should override this method should override
    // this implementation of Abort because canceling pending output reads is not
    // sufficient to abort the connection if there is backpressure.
    public override void Abort(ConnectionAbortedException abortReason)
    {
        Application?.Input.CancelPendingRead();
    }
}