using System;
using System.Linq;
using Microsoft.Framework.Logging;
using LogLevel = OrchardVNext.Logging.LogLevel;

namespace OrchardVNext {
    public static class Logger {
        public static void Debug(string value, params object[] args) {
            Console.WriteLine(value, args);
        }

        public static void Information(string value, params object[] args) {
            if (args.Any())
                Console.WriteLine(value, args);
            else
                Console.WriteLine(value);
        }

        public static void Information(Exception e, string value, params object[] args) {
            Console.WriteLine(value, args);
            Console.Error.WriteLine(e.ToString());
        }

        public static void Warning(string value, params object[] args) {
            Console.WriteLine(value, args);
        }

        public static void Warning(Exception e, string value, params object[] args) {
            Console.WriteLine(value, args);
            Console.Error.WriteLine(e.ToString());
        }

        public static void Error(string value, params object[] args) {
            Console.Error.WriteLine(value, args);
        }

        public static void Error(Exception e, string value, params object[] args) {
            Console.Error.WriteLine(value, args);
            Console.Error.WriteLine(e.ToString());
        }
        public static void TraceError(string message, params object[] args) {
                Console.WriteLine("Error: " + message, args);
        }

        public static void TraceInformation(string message, params object[] args) {
                Console.WriteLine("Information: " + message, args);
        }

        public static void TraceWarning(string message, params object[] args)
        {
            Console.WriteLine("Warning: " + message, args);
        }

        public static bool IsEnabled(LogLevel x) {
            return true;
        }
    }

    public class TestLoggerProvider : ILoggerProvider {
        public ILogger CreateLogger(string name) {
            return new TestLogger(name, true);
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

namespace OrchardVNext.Logging {
    public enum LogLevel {
        Debug,
        Information,
        Warning,
        Error,
        Fatal
    }
}
