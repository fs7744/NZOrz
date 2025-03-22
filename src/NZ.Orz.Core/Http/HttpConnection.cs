using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Connections.Features;
using NZ.Orz.Servers;
using System.Diagnostics;

namespace NZ.Orz.Http;

public class HttpConnection : ITimeoutHandler
{
    private readonly BaseConnectionContext context;
    private readonly ServiceContext serviceContext;
    private readonly TimeProvider timeProvider;
    private readonly TimeoutControl timeoutControl;
    private IRequestProcessor requestProcessor;
#if NET9_0_OR_GREATER
    private readonly Lock _protocolSelectionLock = new();
#else
    private readonly object _protocolSelectionLock = new();
#endif
    private ProtocolSelectionState _protocolSelectionState = ProtocolSelectionState.Initializing;

    public HttpConnection(ConnectionContext context, ServiceContext serviceContext)
    {
        this.context = context;
        this.serviceContext = serviceContext;
        this.timeProvider = serviceContext.TimeProvider;
        this.timeoutControl = new TimeoutControl(this, serviceContext.TimeProvider);
    }

    public HttpConnection(MultiplexedConnectionContext context, ServiceContext serviceContext)
    {
        this.context = context;
        this.serviceContext = serviceContext;
        this.timeoutControl = new TimeoutControl(this, serviceContext.TimeProvider);
    }

    public async Task StartHttpAsync(HttpConnectionDelegate next)
    {
        timeoutControl.Initialize();
        switch (SelectProtocol())
        {
            case GatewayProtocols.HTTP1:
                requestProcessor = new HttpConnection1((ConnectionContext)context, serviceContext, timeoutControl);
                break;

            case GatewayProtocols.HTTP2:
                break;

            case GatewayProtocols.HTTP3:
                break;

            default:
                throw new NotSupportedException($"{nameof(SelectProtocol)} returned something other than Http1, Http2, Http3.");
        }

        if (requestProcessor is not null)
        {
            var connectionHeartbeatFeature = context.GetFeature<IConnectionHeartbeatFeature>();
            var connectionLifetimeNotificationFeature = context.GetFeature<IConnectionLifetimeNotificationFeature>();
            connectionHeartbeatFeature?.OnHeartbeat(state => ((HttpConnection)state).Tick(), this);
            using var shutdownRegistration = connectionLifetimeNotificationFeature?.ConnectionClosedRequested.Register(state => ((HttpConnection)state!).StopProcessingNextRequest(ConnectionEndReason.GracefulAppShutdown), this);
            using var closedRegistration = context.ConnectionClosed.Register(state => ((HttpConnection)state!).OnConnectionClosed(), this);
            await requestProcessor.ProcessRequestsAsync(next);
        }

        //TODO
    }

    private void Tick()
    {
        if (_protocolSelectionState == ProtocolSelectionState.Aborted)
        {
            // It's safe to check for timeouts on a dead connection,
            // but try not to in order to avoid extraneous logs.
            return;
        }

        var timestamp = timeProvider.GetTimestamp();
        timeoutControl.Tick(timestamp);
        requestProcessor!.Tick(timestamp);
    }

    private void StopProcessingNextRequest(ConnectionEndReason reason)
    {
        ProtocolSelectionState previousState;
        lock (_protocolSelectionLock)
        {
            previousState = _protocolSelectionState;
            Debug.Assert(previousState != ProtocolSelectionState.Initializing, "The state should never be initializing");
        }

        switch (previousState)
        {
            case ProtocolSelectionState.Selected:
                requestProcessor!.StopProcessingNextRequest(reason);
                break;

            case ProtocolSelectionState.Aborted:
                break;
        }
    }

    private void OnConnectionClosed()
    {
        ProtocolSelectionState previousState;
        lock (_protocolSelectionLock)
        {
            previousState = _protocolSelectionState;
            Debug.Assert(previousState != ProtocolSelectionState.Initializing, "The state should never be initializing");
        }

        switch (previousState)
        {
            case ProtocolSelectionState.Selected:
                requestProcessor!.OnInputOrOutputCompleted();
                break;

            case ProtocolSelectionState.Aborted:
                break;
        }
    }

    private GatewayProtocols SelectProtocol()
    {
        if (context is ConnectionContext c)
        {
            // todo check http2

            return GatewayProtocols.HTTP1;
        }
        else
        {
            // todo check http3

            return GatewayProtocols.HTTP3;
        }
    }

    public void OnTimeout(TimeoutReason reason)
    {
        //todo
        //switch (reason)
        //{
        //    case TimeoutReason.KeepAlive:
        //        _requestProcessor!.StopProcessingNextRequest(ConnectionEndReason.KeepAliveTimeout);
        //        break;

        //    case TimeoutReason.RequestHeaders:
        //        _requestProcessor!.HandleRequestHeadersTimeout();
        //        break;

        //    case TimeoutReason.ReadDataRate:
        //        _requestProcessor!.HandleReadDataRateTimeout();
        //        break;

        //    case TimeoutReason.WriteDataRate:
        //        Log.ResponseMinimumDataRateNotSatisfied(_context.ConnectionId, _http1Connection?.TraceIdentifier);
        //        Abort(new ConnectionAbortedException(CoreStrings.ConnectionTimedBecauseResponseMininumDataRateNotSatisfied), ConnectionEndReason.MinResponseDataRate);
        //        break;

        //    case TimeoutReason.RequestBodyDrain:
        //    case TimeoutReason.TimeoutFeature:
        //        Abort(new ConnectionAbortedException(CoreStrings.ConnectionTimedOutByServer), ConnectionEndReason.ServerTimeout);
        //        break;

        //    default:
        //        Debug.Assert(false, "Invalid TimeoutReason");
        //        break;
        //}
    }

    private enum ProtocolSelectionState
    {
        Initializing,
        Selected,
        Aborted
    }
}