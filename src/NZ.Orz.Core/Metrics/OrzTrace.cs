using Microsoft.Extensions.Logging;

namespace NZ.Orz.Metrics;

public class OrzTrace : ILogger
{
    private readonly ILogger _generalLogger;

    public OrzTrace(ILoggerFactory loggerFactory)
    {
        _generalLogger = loggerFactory.CreateLogger("NZ.Orz.Server");
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => _generalLogger.Log(logLevel, eventId, state, exception, formatter);

    public bool IsEnabled(LogLevel logLevel) => _generalLogger.IsEnabled(logLevel);

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _generalLogger.BeginScope(state);
}