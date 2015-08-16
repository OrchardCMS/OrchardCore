using System;
using Microsoft.Framework.Logging;

namespace OrchardVNext.Logging {
    public class TestLoggerProvider : ILoggerProvider {
        public ILogger CreateLogger(string name) {
            return new TestLogger(name, true);
        }

        public void Dispose() {
        }
    }

    public class TestLogger : ILogger {
        private object _scope;
        private string _name;
        private bool _enabled;

        public TestLogger(string name,  bool enabled) {
            _name = name;
            _enabled = enabled;
        }

        public string Name { get; set; }

        public void Log(Microsoft.Framework.Logging.LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            Logger.Information(
                string.Format("LogLevel: {0}, EventId: {1}, State: {2}, Exception: {3}, Formatter: {4}, LoggerName: {5}, Scope: {6}",
                 logLevel, eventId, state, exception, formatter, _name, _scope
             ));
        }

        public bool IsEnabled(Microsoft.Framework.Logging.LogLevel logLevel) {
            return _enabled;
        }

        public IDisposable BeginScopeImpl(object state) {
            _scope = state;

            return new TestDisposable();
        }

        private class TestDisposable : IDisposable {
            public void Dispose() {
            }
        }
    }
}