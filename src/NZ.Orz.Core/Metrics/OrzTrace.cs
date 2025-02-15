using Microsoft.Extensions.Logging;

namespace NZ.Orz.Metrics;

public partial class OrzTrace : ILogger
{
    private readonly ILogger _generalLogger;
    private readonly ILogger _connectionsLogger;

    public OrzTrace(ILoggerFactory loggerFactory)
    {
        _generalLogger = loggerFactory.CreateLogger("NZ.Orz.Server");
        _connectionsLogger = loggerFactory.CreateLogger("NZ.Orz.Server.Connections");
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
        [LoggerMessage(23, LogLevel.Critical, @"Connection id ""{ConnectionId}"" application never completed.", EventName = "ApplicationNeverCompleted")]
        public static partial void ApplicationNeverCompleted(ILogger logger, string connectionId);
    }

    #endregion General

    #region ConnectionsLog

    public void NotAllConnectionsClosedGracefully()
    {
        ConnectionsLog.NotAllConnectionsClosedGracefully(_connectionsLogger);
    }

    public void NotAllConnectionsAborted()
    {
        ConnectionsLog.NotAllConnectionsAborted(_connectionsLogger);
    }

    public void ConnectionAccepted(string connectionId)
    {
        ConnectionsLog.ConnectionAccepted(_connectionsLogger, connectionId);
    }

    public void ConnectionStart(string connectionId)
    {
        ConnectionsLog.ConnectionStart(_connectionsLogger, connectionId);
    }

    public void ConnectionStop(string connectionId)
    {
        ConnectionsLog.ConnectionStop(_connectionsLogger, connectionId);
    }

    private static partial class ConnectionsLog
    {
        [LoggerMessage(1, LogLevel.Debug, @"Connection id ""{ConnectionId}"" started.", EventName = "ConnectionStart")]
        public static partial void ConnectionStart(ILogger logger, string connectionId);

        [LoggerMessage(2, LogLevel.Debug, @"Connection id ""{ConnectionId}"" stopped.", EventName = "ConnectionStop")]
        public static partial void ConnectionStop(ILogger logger, string connectionId);

        [LoggerMessage(16, LogLevel.Debug, "Some connections failed to close gracefully during server shutdown.", EventName = "NotAllConnectionsClosedGracefully")]
        public static partial void NotAllConnectionsClosedGracefully(ILogger logger);

        [LoggerMessage(21, LogLevel.Debug, "Some connections failed to abort during server shutdown.", EventName = "NotAllConnectionsAborted")]
        public static partial void NotAllConnectionsAborted(ILogger logger);

        [LoggerMessage(39, LogLevel.Debug, @"Connection id ""{ConnectionId}"" accepted.", EventName = "ConnectionAccepted")]
        public static partial void ConnectionAccepted(ILogger logger, string connectionId);
    }

    #endregion ConnectionsLog
}