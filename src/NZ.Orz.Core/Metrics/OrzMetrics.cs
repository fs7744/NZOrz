using NZ.Orz.Connections;
using NZ.Orz.Connections.Features;
using NZ.Orz.Sockets;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Sockets;

namespace NZ.Orz.Metrics;

public sealed class OrzMetrics
{
    // Follows boundaries from http.server.request.duration/http.client.request.duration
    public static readonly IReadOnlyList<double> ShortSecondsBucketBoundaries = [0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10];

    // Not based on a standard. Larger bucket sizes for longer lasting operations, e.g. HTTP connection duration. See https://github.com/open-telemetry/semantic-conventions/issues/336
    public static readonly IReadOnlyList<double> LongSecondsBucketBoundaries = [0.01, 0.02, 0.05, 0.1, 0.2, 0.5, 1, 2, 5, 10, 30, 60, 120, 300];

    public const string MeterName = "NZ.Orz.Server";
    public const string ErrorTypeAttributeName = "error.type";
    private readonly Meter _meter;
    private readonly UpDownCounter<long> _activeConnectionsCounter;
    private readonly Histogram<double> _connectionDuration;
    private readonly Counter<long> _rejectedConnectionsCounter;
    private readonly UpDownCounter<long> _queuedConnectionsCounter;
    private readonly UpDownCounter<long> _queuedRequestsCounter;
    private readonly UpDownCounter<long> _currentUpgradedRequestsCounter;
    private readonly Histogram<double> _tlsHandshakeDuration;
    private readonly UpDownCounter<long> _activeTlsHandshakesCounter;

    public OrzMetrics(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create(MeterName);
        _activeConnectionsCounter = _meter.CreateUpDownCounter<long>(
            "orz.active_connections",
            unit: "{connection}",
            description: "Number of connections that are currently active on the server.");

        _connectionDuration = _meter.CreateHistogram<double>(
            "orz.connection.duration",
            unit: "s",
            description: "The duration of connections on the server.",
            advice: new InstrumentAdvice<double> { HistogramBucketBoundaries = LongSecondsBucketBoundaries });

        _rejectedConnectionsCounter = _meter.CreateCounter<long>(
           "orz.rejected_connections",
            unit: "{connection}",
            description: "Number of connections rejected by the server. Connections are rejected when the currently active count exceeds the value configured with MaxConcurrentConnections.");

        _queuedConnectionsCounter = _meter.CreateUpDownCounter<long>(
           "orz.queued_connections",
            unit: "{connection}",
            description: "Number of connections that are currently queued and are waiting to start.");

        _queuedRequestsCounter = _meter.CreateUpDownCounter<long>(
           "orz.queued_requests",
            unit: "{request}",
            description: "Number of HTTP requests on multiplexed connections (HTTP/2 and HTTP/3) that are currently queued and are waiting to start.");

        _currentUpgradedRequestsCounter = _meter.CreateUpDownCounter<long>(
           "orz.upgraded_connections",
            unit: "{connection}",
            description: "Number of HTTP connections that are currently upgraded (WebSockets). The number only tracks HTTP/1.1 connections.");

        _tlsHandshakeDuration = _meter.CreateHistogram<double>(
            "orz.tls_handshake.duration",
            unit: "s",
            description: "The duration of TLS handshakes on the server.",
            advice: new InstrumentAdvice<double> { HistogramBucketBoundaries = ShortSecondsBucketBoundaries });

        _activeTlsHandshakesCounter = _meter.CreateUpDownCounter<long>(
           "orz.active_tls_handshakes",
            unit: "{handshake}",
            description: "Number of TLS handshakes that are currently in progress on the server.");
    }

    public void ConnectionQueuedStart(ConnectionMetricsContext metricsContext)
    {
        if (metricsContext.QueuedConnectionsCounterEnabled)
        {
            var tags = new TagList();
            InitializeConnectionTags(ref tags, metricsContext);
            _queuedConnectionsCounter.Add(1, tags);
        }
    }

    public void ConnectionQueuedStop(ConnectionMetricsContext metricsContext)
    {
        if (metricsContext.QueuedConnectionsCounterEnabled)
        {
            var tags = new TagList();
            InitializeConnectionTags(ref tags, metricsContext);
            _queuedConnectionsCounter.Add(-1, tags);
        }
    }

    public void ConnectionStart(ConnectionMetricsContext metricsContext)
    {
        if (metricsContext.CurrentConnectionsCounterEnabled)
        {
            var tags = new TagList();
            InitializeConnectionTags(ref tags, metricsContext);
            _activeConnectionsCounter.Add(1, tags);
        }
    }

    public void ConnectionStop(ConnectionMetricsContext metricsContext, Exception? exception, List<KeyValuePair<string, object?>>? customTags, long startTimestamp, long currentTimestamp)
    {
        if (metricsContext.CurrentConnectionsCounterEnabled || metricsContext.ConnectionDurationEnabled)
        {
            var tags = new TagList();
            InitializeConnectionTags(ref tags, metricsContext);

            if (metricsContext.CurrentConnectionsCounterEnabled)
            {
                // Decrease in connections counter must match tags from increase. No custom tags.
                _activeConnectionsCounter.Add(-1, tags);
            }

            if (metricsContext.ConnectionDurationEnabled)
            {
                // Add custom tags for duration.
                if (customTags != null)
                {
                    for (var i = 0; i < customTags.Count; i++)
                    {
                        tags.Add(customTags[i]);
                    }
                }

                // Check if there is an end reason on the context. For example, the connection could have been aborted by shutdown.
                if (metricsContext.ConnectionEndReason is { } reason && TryGetErrorType(reason, out var errorValue))
                {
                    tags.TryAddTag(ErrorTypeAttributeName, errorValue);
                }
                else if (exception != null)
                {
                    tags.TryAddTag(ErrorTypeAttributeName, exception.GetType().FullName);
                }

                var duration = Stopwatch.GetElapsedTime(startTimestamp, currentTimestamp);
                _connectionDuration.Record(duration.TotalSeconds, tags);
            }
        }
    }

    private static void InitializeConnectionTags(ref TagList tags, in ConnectionMetricsContext metricsContext)
    {
        var localEndpoint = metricsContext.ConnectionContext.LocalEndPoint;
        if (localEndpoint is IPEndPoint localIPEndPoint)
        {
            tags.Add("server.address", localIPEndPoint.Address.ToString());
            tags.Add("server.port", localIPEndPoint.Port);

            switch (localIPEndPoint.Address.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    tags.Add("network.type", "ipv4");
                    break;

                case AddressFamily.InterNetworkV6:
                    tags.Add("network.type", "ipv6");
                    break;
            }

            tags.Add("network.transport", metricsContext.ConnectionContext is MultiplexedConnectionContext || metricsContext.ConnectionContext is UdpConnectionContext ? "udp" : "tcp");
        }
        else if (localEndpoint is UnixDomainSocketEndPoint udsEndPoint)
        {
            tags.Add("server.address", udsEndPoint.ToString());
            tags.Add("network.transport", "unix");
        }
        else if (localEndpoint is NamedPipeEndPoint namedPipeEndPoint)
        {
            tags.Add("server.address", namedPipeEndPoint.ToString());
            tags.Add("network.transport", "pipe");
        }
        else if (localEndpoint != null)
        {
            tags.Add("server.address", localEndpoint.ToString());
            tags.Add("network.transport", localEndpoint.AddressFamily.ToString());
        }
    }

    public ConnectionMetricsContext CreateContext(BaseConnectionContext connection)
    {
        return new ConnectionMetricsContext
        {
            ConnectionContext = connection,
            CurrentConnectionsCounterEnabled = _activeConnectionsCounter.Enabled,
            ConnectionDurationEnabled = _connectionDuration.Enabled,
            QueuedConnectionsCounterEnabled = _queuedConnectionsCounter.Enabled,
            QueuedRequestsCounterEnabled = _queuedRequestsCounter.Enabled,
            CurrentUpgradedRequestsCounterEnabled = _currentUpgradedRequestsCounter.Enabled,
            CurrentTlsHandshakesCounterEnabled = _activeTlsHandshakesCounter.Enabled
        };
    }

    internal static void AddConnectionEndReason(ConnectionMetricsContext? context, ConnectionEndReason reason, bool overwrite = false)
    {
        if (context != null)
        {
            // Set end reason when either:
            // - Overwrite is true. For example, AppShutdownTimeout reason is forced when shutting down
            //   the app reguardless of whether there is already a value.
            // - New reason is an error type and there isn't already an error type set.
            //   In other words, first error wins.
            if (overwrite)
            {
                Debug.Assert(TryGetErrorType(reason, out _), "Overwrite should only be set for an error reason.");
                context.ConnectionEndReason = reason;
            }
            else if (TryGetErrorType(reason, out _))
            {
                if (context.ConnectionEndReason is null)
                {
                    context.ConnectionEndReason = reason;
                }
            }
        }
    }

    internal static bool TryGetErrorType(ConnectionEndReason reason, [NotNullWhen(true)] out string? errorTypeValue)
    {
        errorTypeValue = reason switch
        {
            ConnectionEndReason.Unset => null, // Not an error
            ConnectionEndReason.ClientGoAway => null, // Not an error
            ConnectionEndReason.TransportCompleted => null, // Not an error
            ConnectionEndReason.GracefulAppShutdown => null, // Not an error
            ConnectionEndReason.RequestNoKeepAlive => null, // Not an error
            ConnectionEndReason.ResponseNoKeepAlive => null, // Not an error
            ConnectionEndReason.ErrorAfterStartingResponse => "error_after_starting_response",
            ConnectionEndReason.ConnectionReset => "connection_reset",
            ConnectionEndReason.FlowControlWindowExceeded => "flow_control_window_exceeded",
            ConnectionEndReason.KeepAliveTimeout => "keep_alive_timeout",
            ConnectionEndReason.InsufficientTlsVersion => "insufficient_tls_version",
            ConnectionEndReason.InvalidHandshake => "invalid_handshake",
            ConnectionEndReason.InvalidStreamId => "invalid_stream_id",
            ConnectionEndReason.FrameAfterStreamClose => "frame_after_stream_close",
            ConnectionEndReason.UnknownStream => "unknown_stream",
            ConnectionEndReason.UnexpectedFrame => "unexpected_frame",
            ConnectionEndReason.InvalidFrameLength => "invalid_frame_length",
            ConnectionEndReason.InvalidDataPadding => "invalid_data_padding",
            ConnectionEndReason.InvalidRequestHeaders => "invalid_request_headers",
            ConnectionEndReason.StreamResetLimitExceeded => "stream_reset_limit_exceeded",
            ConnectionEndReason.InvalidWindowUpdateSize => "invalid_window_update_size",
            ConnectionEndReason.StreamSelfDependency => "stream_self_dependency",
            ConnectionEndReason.InvalidSettings => "invalid_settings",
            ConnectionEndReason.MissingStreamEnd => "missing_stream_end",
            ConnectionEndReason.MaxFrameLengthExceeded => "max_frame_length_exceeded",
            ConnectionEndReason.ErrorReadingHeaders => "error_reading_headers",
            ConnectionEndReason.ErrorWritingHeaders => "error_writing_headers",
            ConnectionEndReason.OtherError => "other_error",
            ConnectionEndReason.InvalidHttpVersion => "invalid_http_version",
            ConnectionEndReason.RequestHeadersTimeout => "request_headers_timeout",
            ConnectionEndReason.MinRequestBodyDataRate => "min_request_body_data_rate",
            ConnectionEndReason.MinResponseDataRate => "min_response_data_rate",
            ConnectionEndReason.FlowControlQueueSizeExceeded => "flow_control_queue_size_exceeded",
            ConnectionEndReason.OutputQueueSizeExceeded => "output_queue_size_exceeded",
            ConnectionEndReason.ClosedCriticalStream => "closed_critical_stream",
            ConnectionEndReason.AbortedByApp => "aborted_by_app",
            ConnectionEndReason.WriteCanceled => "write_canceled",
            ConnectionEndReason.InvalidBodyReaderState => "invalid_body_reader_state",
            ConnectionEndReason.ServerTimeout => "server_timeout",
            ConnectionEndReason.StreamCreationError => "stream_creation_error",
            ConnectionEndReason.IOError => "io_error",
            ConnectionEndReason.AppShutdownTimeout => "app_shutdown_timeout",
            ConnectionEndReason.TlsHandshakeFailed => "tls_handshake_failed",
            ConnectionEndReason.InvalidRequestLine => "invalid_request_line",
            ConnectionEndReason.TlsNotSupported => "tls_not_supported",
            ConnectionEndReason.MaxRequestBodySizeExceeded => "max_request_body_size_exceeded",
            ConnectionEndReason.UnexpectedEndOfRequestContent => "unexpected_end_of_request_content",
            ConnectionEndReason.MaxConcurrentConnectionsExceeded => "max_concurrent_connections_exceeded",
            ConnectionEndReason.MaxRequestHeadersTotalSizeExceeded => "max_request_headers_total_size_exceeded",
            ConnectionEndReason.MaxRequestHeaderCountExceeded => "max_request_header_count_exceeded",
            ConnectionEndReason.ResponseContentLengthMismatch => "response_content_length_mismatch",
            _ => throw new InvalidOperationException($"Unable to calculate whether {reason} resolves to error.type value.")
        };

        return errorTypeValue != null;
    }
}