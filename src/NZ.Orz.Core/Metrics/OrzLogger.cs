using Microsoft.Extensions.Logging;
using NZ.Orz.Config;
using NZ.Orz.Http.Exceptions;
using System.Net;

namespace NZ.Orz.Metrics;

public partial class OrzLogger : ILogger
{
    private readonly ILogger _generalLogger;
    private readonly ILogger _connectionsLogger;
    private readonly ILogger _socketlogger;
    private readonly ILogger _proxylogger;
    private readonly ILogger _httplogger;

    public OrzLogger(ILoggerFactory loggerFactory)
    {
        _generalLogger = loggerFactory.CreateLogger("NZ.Orz.Server");
        _connectionsLogger = loggerFactory.CreateLogger("NZ.Orz.Server.Connections");
        _socketlogger = loggerFactory.CreateLogger("NZ.Orz.Server.Transport.Sockets");
        _proxylogger = loggerFactory.CreateLogger("NZ.Orz.Server.ReverseProxy");
        _httplogger = loggerFactory.CreateLogger("NZ.Orz.Server.Http");
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => _generalLogger.Log(logLevel, eventId, state, exception, formatter);

    public bool IsEnabled(LogLevel logLevel) => _generalLogger.IsEnabled(logLevel);

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _generalLogger.BeginScope(state);

    #region General

    public void ApplicationNeverCompleted(string connectionId)
    {
        GeneralLog.ApplicationNeverCompleted(_generalLogger, connectionId);
    }

    private static partial class GeneralLog
    {
        [LoggerMessage(13, LogLevel.Critical, @"Connection id ""{ConnectionId}"" application never completed.", EventName = "ApplicationNeverCompleted")]
        public static partial void ApplicationNeverCompleted(ILogger logger, string connectionId);
    }

    #endregion General

    #region ConnectionsLog

    public void UnexpectedException(string msg, Exception ex)
    {
        GeneralLog.UnexpectedException(_generalLogger, msg, ex);
    }

    public void NotAllConnectionsClosedGracefully()
    {
        GeneralLog.NotAllConnectionsClosedGracefully(_connectionsLogger);
    }

    public void NotAllConnectionsAborted()
    {
        GeneralLog.NotAllConnectionsAborted(_connectionsLogger);
    }

    public void ConnectionAccepted(string connectionId)
    {
        GeneralLog.ConnectionAccepted(_connectionsLogger, connectionId);
    }

    public void ConnectionStart(string connectionId)
    {
        GeneralLog.ConnectionStart(_connectionsLogger, connectionId);
    }

    public void ConnectionStop(string connectionId)
    {
        GeneralLog.ConnectionStop(_connectionsLogger, connectionId);
    }

    public void ApplicationError(string connectionId, string traceIdentifier, Exception ex)
    {
        GeneralLog.ApplicationError(_httplogger, connectionId, traceIdentifier, ex);
    }

    public void ConnectionBadRequest(string connectionId, BadHttpRequestException ex)
    {
        GeneralLog.ConnectionBadRequest(_httplogger, connectionId, ex.Message, ex);
    }

    public void RequestBodyDrainBodyReaderInvalidState(string connectionId, string traceIdentifier, Exception ex)
    {
        GeneralLog.RequestBodyDrainBodyReaderInvalidState(_httplogger, connectionId, traceIdentifier, ex);
    }

    public void RequestBodyNotEntirelyRead(string connectionId, string traceIdentifier)
    {
        GeneralLog.RequestBodyNotEntirelyRead(_httplogger, connectionId, traceIdentifier);
    }

    public void RequestBodyDrainTimedOut(string connectionId, string traceIdentifier)
    {
        GeneralLog.RequestBodyDrainTimedOut(_httplogger, connectionId, traceIdentifier);
    }

    public void RequestProcessingError(string connectionId, Exception ex)
    {
        GeneralLog.RequestProcessingError(_httplogger, connectionId, ex);
    }

    public void RequestAborted(string connectionId, string traceIdentifier)
    {
        GeneralLog.RequestAbortedException(_httplogger, connectionId, traceIdentifier);
    }

    public void ConnectionKeepAlive(string connectionId)
    {
        GeneralLog.ConnectionKeepAlive(_httplogger, connectionId);
    }

    public void ConnectionHeadResponseBodyWrite(string connectionId, long count)
    {
        GeneralLog.ConnectionHeadResponseBodyWrite(_httplogger, connectionId, count);
    }

    private static partial class GeneralLog
    {
        [LoggerMessage(0, LogLevel.Error, @"Unexpected exception {Msg}.", EventName = "UnexpectedException", SkipEnabledCheck = true)]
        public static partial void UnexpectedException(ILogger logger, string msg, Exception ex);

        [LoggerMessage(1, LogLevel.Debug, @"Connection id ""{ConnectionId}"" started.", EventName = "ConnectionStart")]
        public static partial void ConnectionStart(ILogger logger, string connectionId);

        [LoggerMessage(2, LogLevel.Debug, @"Connection id ""{ConnectionId}"" stopped.", EventName = "ConnectionStop")]
        public static partial void ConnectionStop(ILogger logger, string connectionId);

        [LoggerMessage(3, LogLevel.Debug, "Some connections failed to close gracefully during server shutdown.", EventName = "NotAllConnectionsClosedGracefully")]
        public static partial void NotAllConnectionsClosedGracefully(ILogger logger);

        [LoggerMessage(4, LogLevel.Debug, "Some connections failed to abort during server shutdown.", EventName = "NotAllConnectionsAborted")]
        public static partial void NotAllConnectionsAborted(ILogger logger);

        [LoggerMessage(5, LogLevel.Debug, @"Connection id ""{ConnectionId}"" accepted.", EventName = "ConnectionAccepted")]
        public static partial void ConnectionAccepted(ILogger logger, string connectionId);

        [LoggerMessage(31, LogLevel.Error, @"Connection id ""{ConnectionId}"", Request id ""{TraceIdentifier}"": An unhandled exception was thrown by the application.", EventName = "ApplicationError")]
        public static partial void ApplicationError(ILogger logger, string connectionId, string traceIdentifier, Exception ex);

        [LoggerMessage(32, LogLevel.Debug, @"Connection id ""{ConnectionId}"" bad request data: ""{message}""", EventName = "ConnectionBadRequest")]
        public static partial void ConnectionBadRequest(ILogger logger, string connectionId, string message, BadHttpRequestException ex);

        [LoggerMessage(33, LogLevel.Error, @"Connection id ""{ConnectionId}"", Request id ""{TraceIdentifier}"": automatic draining of the request body failed because the body reader is in an invalid state.", EventName = "RequestBodyDrainBodyReaderInvalidState")]
        public static partial void RequestBodyDrainBodyReaderInvalidState(ILogger logger, string connectionId, string traceIdentifier, Exception ex);

        [LoggerMessage(34, LogLevel.Information, @"Connection id ""{ConnectionId}"", Request id ""{TraceIdentifier}"": the application completed without reading the entire request body.", EventName = "RequestBodyNotEntirelyRead")]
        public static partial void RequestBodyNotEntirelyRead(ILogger logger, string connectionId, string traceIdentifier);

        [LoggerMessage(35, LogLevel.Information, @"Connection id ""{ConnectionId}"", Request id ""{TraceIdentifier}"": automatic draining of the request body timed out after taking over 5 seconds.", EventName = "RequestBodyDrainTimedOut")]
        public static partial void RequestBodyDrainTimedOut(ILogger logger, string connectionId, string traceIdentifier);

        [LoggerMessage(36, LogLevel.Debug, @"Connection id ""{ConnectionId}"" request processing ended abnormally.", EventName = "RequestProcessingError")]
        public static partial void RequestProcessingError(ILogger logger, string connectionId, Exception ex);

        [LoggerMessage(37, LogLevel.Debug, @"Connection id ""{ConnectionId}"", Request id ""{TraceIdentifier}"": The request was aborted by the client.", EventName = "RequestAborted")]
        public static partial void RequestAbortedException(ILogger logger, string connectionId, string traceIdentifier);

        [LoggerMessage(38, LogLevel.Debug, @"Connection id ""{ConnectionId}"" completed keep alive response.", EventName = "ConnectionKeepAlive")]
        public static partial void ConnectionKeepAlive(ILogger logger, string connectionId);

        [LoggerMessage(39, LogLevel.Debug, @"Connection id ""{ConnectionId}"" write of ""{count}"" body bytes to non-body HEAD response.", EventName = "ConnectionHeadResponseBodyWrite")]
        public static partial void ConnectionHeadResponseBodyWrite(ILogger logger, string connectionId, long count);
    }

    #endregion ConnectionsLog

    #region SocketsLog

    public void ConnectionReadFin(string connectionId)
    {
        GeneralLog.ConnectionReadFinCore(_socketlogger, connectionId);
    }

    public void ConnectionWriteFin(string connectionId, string reason)
    {
        GeneralLog.ConnectionWriteFinCore(_socketlogger, connectionId, reason);
    }

    public void ConnectionWriteRst(string connectionId, string reason)
    {
        GeneralLog.ConnectionWriteRstCore(_socketlogger, connectionId, reason);
    }

    public void ConnectionError(string connectionId, Exception ex)
    {
        GeneralLog.ConnectionErrorCore(_socketlogger, connectionId, ex);
    }

    public void ConnectionReset(string connectionId)
    {
        GeneralLog.ConnectionReset(_socketlogger, connectionId);
    }

    public void ConnectionPause(string connectionId)
    {
        GeneralLog.ConnectionPauseCore(_socketlogger, connectionId);
    }

    public void ConnectionResume(string connectionId)
    {
        GeneralLog.ConnectionResumeCore(_socketlogger, connectionId);
    }

    public void ConnectionCompleteFeatureError(Exception ex)
    {
        GeneralLog.ConnectionCompleteFeatureError(_socketlogger, ex);
    }

    private static partial class GeneralLog
    {
        [LoggerMessage(6, LogLevel.Debug, @"Connection id ""{ConnectionId}"" received FIN.", EventName = "ConnectionReadFin")]
        public static partial void ConnectionReadFinCore(ILogger logger, string connectionId);

        [LoggerMessage(7, LogLevel.Debug, @"Connection id ""{ConnectionId}"" sending FIN because: ""{Reason}""", EventName = "ConnectionWriteFin")]
        public static partial void ConnectionWriteFinCore(ILogger logger, string connectionId, string reason);

        [LoggerMessage(8, LogLevel.Debug, @"Connection id ""{ConnectionId}"" sending RST because: ""{Reason}""", EventName = "ConnectionWriteRst")]
        public static partial void ConnectionWriteRstCore(ILogger logger, string connectionId, string reason);

        [LoggerMessage(9, LogLevel.Debug, @"Connection id ""{ConnectionId}"" communication error.", EventName = "ConnectionError")]
        public static partial void ConnectionErrorCore(ILogger logger, string connectionId, Exception ex);

        [LoggerMessage(10, LogLevel.Debug, @"Connection id ""{ConnectionId}"" reset.", EventName = "ConnectionReset")]
        public static partial void ConnectionReset(ILogger logger, string connectionId);

        [LoggerMessage(11, LogLevel.Debug, @"Connection id ""{ConnectionId}"" paused.", EventName = "ConnectionPause")]
        public static partial void ConnectionPauseCore(ILogger logger, string connectionId);

        [LoggerMessage(12, LogLevel.Debug, @"Connection id ""{ConnectionId}"" resumed.", EventName = "ConnectionResume")]
        public static partial void ConnectionResumeCore(ILogger logger, string connectionId);

        [LoggerMessage(14, LogLevel.Error, "An error occurred running an IConnectionCompleteFeature.OnCompleted callback.", EventName = "ConnectionCompleteFeatureError", SkipEnabledCheck = true)]
        public static partial void ConnectionCompleteFeatureError(ILogger logger, Exception ex);
    }

    #endregion SocketsLog

    #region ReverseProxy

    public void NotFoundAvailableUpstream(string clusterId)
    {
        GeneralLog.NotFoundAvailableUpstream(_proxylogger, clusterId);
    }

    public void NotFoundRouteL4(EndPoint endPoint)
    {
        GeneralLog.NotFoundRouteL4(_proxylogger, endPoint);
    }

    public void NotFoundRouteSni(string host)
    {
        GeneralLog.NotFoundRouteSni(_proxylogger, host);
    }

    public void StopEndpointsInfo(List<ListenOptions> endPoints)
    {
        if (_proxylogger.IsEnabled(LogLevel.Information))
            GeneralLog.StopEndpointsInfo(_proxylogger, string.Join(',', endPoints));
    }

    public void StartEndpointsInfo(List<ListenOptions> endPoints)
    {
        if (_proxylogger.IsEnabled(LogLevel.Information))
            GeneralLog.StartEndpointsInfo(_proxylogger, string.Join(',', endPoints));
    }

    public void BindListenOptionsError(ListenOptions endPoint, Exception ex)
    {
        GeneralLog.BindListenOptionsError(_proxylogger, endPoint, ex);
    }

    public void RemoveErrorCluster(string clusterId)
    {
        GeneralLog.RemoveErrorCluster(_proxylogger, clusterId);
    }

    public void RemoveErrorRoute(string routeId)
    {
        GeneralLog.RemoveErrorRoute(_proxylogger, routeId);
    }

    public void RemoveErrorListenOptions(ListenOptions endPoint)
    {
        GeneralLog.RemoveErrorListenOptions(_proxylogger, endPoint);
    }

    public void SocketConnectionCheckFailed(EndPoint endPoint, Exception ex)
    {
        GeneralLog.SocketConnectionCheckFailed(_proxylogger, endPoint, ex.Message);
    }

    public void NotFoundActiveHealthCheckPolicy(string policy)
    {
        GeneralLog.NotFoundActiveHealthCheckPolicy(_proxylogger, policy);
    }

    public void ConfigError(string msg)
    {
        GeneralLog.ConfigError(_proxylogger, msg);
    }

    public void ProxyBegin(string routeId)
    {
        GeneralLog.ProxyBegin(_proxylogger, routeId);
    }

    public void ProxyEnd(string routeId)
    {
        GeneralLog.ProxyEnd(_proxylogger, routeId);
    }

    public void ProxyTimeout(string routeId, TimeSpan time)
    {
        GeneralLog.ProxyTimeout(_proxylogger, routeId, time);
    }

    public void ConnectUpstreamTimeout(string routeId)
    {
        GeneralLog.ConnectUpstreamTimeout(_proxylogger, routeId);
    }

    private static partial class GeneralLog
    {
        [LoggerMessage(15, LogLevel.Warning, @"Not found available upstream for cluster ""{ClusterId}"".", EventName = "NotFoundAvailableUpstream")]
        public static partial void NotFoundAvailableUpstream(ILogger logger, string clusterId);

        [LoggerMessage(16, LogLevel.Warning, @"Not found l4 route for ""{EndPoint}"".", EventName = "NotFoundRouteL4")]
        public static partial void NotFoundRouteL4(ILogger logger, EndPoint endPoint);

        [LoggerMessage(17, LogLevel.Information, @"Config changed. Stopping the following endpoints: {Endpoints}.", EventName = "StopEndpointsInfo", SkipEnabledCheck = true)]
        public static partial void StopEndpointsInfo(ILogger logger, string endpoints);

        [LoggerMessage(18, LogLevel.Information, @"Config changed. Starting the following endpoints: {endpoints}", EventName = "StartEndpointsInfo", SkipEnabledCheck = true)]
        public static partial void StartEndpointsInfo(ILogger logger, string endpoints);

        [LoggerMessage(19, LogLevel.Critical, @"Unable to bind to {Endpoint} on config reload.", EventName = "BindListenOptionsError")]
        public static partial void BindListenOptionsError(ILogger logger, ListenOptions endpoint, Exception ex);

        [LoggerMessage(20, LogLevel.Warning, @"Ingore error cluster config {clusterId}.", EventName = "RemoveErrorCluster")]
        public static partial void RemoveErrorCluster(ILogger logger, string clusterId);

        [LoggerMessage(21, LogLevel.Warning, @"Ingore error route config {routeId}.", EventName = "RemoveErrorRoute")]
        public static partial void RemoveErrorRoute(ILogger logger, string routeId);

        [LoggerMessage(22, LogLevel.Warning, @"Ingore error listen options config {endPoint}.", EventName = "RemoveErrorListenOptions")]
        public static partial void RemoveErrorListenOptions(ILogger logger, ListenOptions endPoint);

        [LoggerMessage(23, LogLevel.Warning, @"Active health failed, can not connect socket {endPoint} {ex}.", EventName = "SocketConnectionCheckFailed")]
        public static partial void SocketConnectionCheckFailed(ILogger logger, EndPoint endPoint, string ex);

        [LoggerMessage(24, LogLevel.Warning, @"Not found active health check policy {policy}.", EventName = "NotFoundActiveHealthCheckPolicy")]
        public static partial void NotFoundActiveHealthCheckPolicy(ILogger logger, string policy);

        [LoggerMessage(25, LogLevel.Warning, @"Config error: {msg}.", EventName = "ConfigError")]
        public static partial void ConfigError(ILogger logger, string msg);

        [LoggerMessage(26, LogLevel.Information, @"Begin proxy for route {routeId}.", EventName = "ProxyBegin")]
        public static partial void ProxyBegin(ILogger logger, string routeId);

        [LoggerMessage(27, LogLevel.Information, @"End proxy for route {routeId}.", EventName = "ProxyEnd")]
        public static partial void ProxyEnd(ILogger logger, string routeId);

        [LoggerMessage(28, LogLevel.Information, @"Proxy timeout ({time}) for route {routeId}.", EventName = "ProxyTimeout")]
        public static partial void ProxyTimeout(ILogger logger, string routeId, TimeSpan time);

        [LoggerMessage(29, LogLevel.Information, @"Connect upstream timeout for route {routeId}.", EventName = "ConnectUpstreamTimeout")]
        public static partial void ConnectUpstreamTimeout(ILogger logger, string routeId);

        [LoggerMessage(30, LogLevel.Warning, @"Not found sni route for ""{EndPoint}"".", EventName = "NotFoundRouteSni")]
        public static partial void NotFoundRouteSni(ILogger logger, string endPoint);
    }

    #endregion ReverseProxy
}