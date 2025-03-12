using Microsoft.Extensions.Logging;
using NZ.Orz.Connections.Exceptions;
using NZ.Orz.Connections.Features;
using NZ.Orz.Metrics;
using NZ.Orz.Servers;
using System.Diagnostics;

namespace NZ.Orz.Connections;

internal sealed class OrzConnection<T> : OrzConnection, IThreadPoolWorkItem where T : BaseConnectionContext
{
    private readonly Func<T, Task> _connectionDelegate;
    private readonly T _transportConnection;

    public OrzConnection(long id,
                             ServiceContext serviceContext,
                             TransportConnectionManager transportConnectionManager,
                             Func<T, Task> connectionDelegate,
                             T connectionContext,
                             OrzLogger logger,
                             ConnectionMetricsContext connectionMetricsContext)
        : base(id, serviceContext, transportConnectionManager, logger, connectionMetricsContext)
    {
        _connectionDelegate = connectionDelegate;
        _transportConnection = connectionContext;
        connectionContext.Parameters.SetFeature<IConnectionHeartbeatFeature>(this);
        connectionContext.Parameters.SetFeature<IConnectionCompleteFeature>(this);
        connectionContext.Parameters.SetFeature<IConnectionLifetimeNotificationFeature>(this);
        connectionContext.Parameters.SetFeature<IConnectionMetricsContextFeature>(this);
    }

    private OrzMetrics Metrics => _serviceContext.Metrics;
    public override BaseConnectionContext TransportConnection => _transportConnection;

    void IThreadPoolWorkItem.Execute()
    {
        _ = ExecuteAsync();
    }

    internal async Task ExecuteAsync()
    {
        var connectionContext = _transportConnection;
        var startTimestamp = 0L;
        ConnectionMetricsTagsFeature? metricsTagsFeature = null;
        Exception? unhandledException = null;

        if (MetricsContext.ConnectionDurationEnabled)
        {
            metricsTagsFeature = new ConnectionMetricsTagsFeature();
            connectionContext.Parameters.SetFeature<IConnectionMetricsTagsFeature>(metricsTagsFeature);

            startTimestamp = Stopwatch.GetTimestamp();
        }

        try
        {
            Metrics.ConnectionQueuedStop(MetricsContext);

            Logger.ConnectionStart(connectionContext.ConnectionId);
            Metrics.ConnectionStart(MetricsContext);

            using (BeginConnectionScope(connectionContext))
            {
                try
                {
                    await _connectionDelegate(connectionContext);
                }
                catch (ConnectionAbortedException e)
                {
                    unhandledException = e;
                    Logger.LogInformation(e, e.Message);
                }
                catch (Exception ex)
                {
                    unhandledException = ex;
                    Logger.UnexpectedException($"while processing {connectionContext.ConnectionId}", ex);
                }
            }
        }
        finally
        {
            await FireOnCompletedAsync();

            var currentTimestamp = 0L;
            if (MetricsContext.ConnectionDurationEnabled)
            {
                currentTimestamp = Stopwatch.GetTimestamp();
            }

            Logger.ConnectionStop(connectionContext.ConnectionId);
            Metrics.ConnectionStop(MetricsContext, unhandledException, metricsTagsFeature?.TagsList, startTimestamp, currentTimestamp);

            // Dispose the transport connection, this needs to happen before removing it from the
            // connection manager so that we only signal completion of this connection after the transport
            // is properly torn down.
            await connectionContext.DisposeAsync();

            _transportConnectionManager.RemoveConnection(_id);
        }
    }

    private sealed class ConnectionMetricsTagsFeature : IConnectionMetricsTagsFeature
    {
        ICollection<KeyValuePair<string, object?>> IConnectionMetricsTagsFeature.Tags => TagsList;

        public List<KeyValuePair<string, object?>> TagsList { get; } = new List<KeyValuePair<string, object?>>();
    }
}