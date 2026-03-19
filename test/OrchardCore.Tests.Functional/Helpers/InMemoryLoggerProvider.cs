using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Tests.Functional.Helpers;

public sealed class InMemoryLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentBag<LogEntry> _entries;

    public InMemoryLoggerProvider(ConcurrentBag<LogEntry> entries)
    {
        _entries = entries;
    }

    public ILogger CreateLogger(string categoryName) => new InMemoryLogger(categoryName, _entries);

    public void Dispose() { }

    private sealed class InMemoryLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly ConcurrentBag<LogEntry> _entries;

        public InMemoryLogger(string categoryName, ConcurrentBag<LogEntry> entries)
        {
            _categoryName = categoryName;
            _entries = entries;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            _entries.Add(new LogEntry
            {
                LogLevel = logLevel,
                Category = _categoryName,
                Message = formatter(state, exception),
                Exception = exception,
            });
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}

public sealed class LogEntry
{
    public LogLevel LogLevel { get; init; }
    public string Category { get; init; }
    public string Message { get; init; }
    public Exception Exception { get; init; }
}
