using Microsoft.Extensions.Logging;
using System;

namespace OrchardCore.Tests
{
    public class StubLoggerFactory : ILoggerFactory
    {
        public LogLevel MinimumLevel
        {
            get
            {
                return LogLevel.Critical;
            }

            set
            {
            }
        }

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new NullLogger();
        }

        public void Dispose()
        {
        }
    }

    public class NullLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return new FakeScope();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
        }

        private class FakeScope : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }

    public class NullLogger<T> : NullLogger, ILogger<T>
    {
    }
}