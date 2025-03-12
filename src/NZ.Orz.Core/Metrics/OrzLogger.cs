using Microsoft.Extensions.Logging;
using NZ.Orz.Config;
using System.Net;

namespace NZ.Orz.Metrics;

public partial class OrzLogger : ILogger
{
    private readonly ILogger _generalLogger;
    private readonly ILogger _connectionsLogger;
    private readonly ILogger _socketlogger;
    private readonly ILogger _proxylogger;

    public OrzLogger(ILoggerFactory loggerFactory)
    {
        _generalLogger = loggerFactory.CreateLogger("NZ.Orz.Server");
        _connectionsLogger = loggerFactory.CreateLogger("NZ.Orz.Server.Connections");
        _socketlogger = loggerFactory.CreateLogger("NZ.Orz.Server.Transport.Sockets");
        _proxylogger = loggerFactory.CreateLogger("NZ.Orz.Server.ReverseProxy");
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

        [LoggerMessage(16, LogLevel.Warning, @"Not found route for ""{EndPoint}"".", EventName = "NotFoundRouteL4")]
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
    }

    #endregion ReverseProxy
}