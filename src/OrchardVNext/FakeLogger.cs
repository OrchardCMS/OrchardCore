using Microsoft.Framework.Logging;
using OrchardVNext.Logging;
using System;
using System.Linq;

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

        public static bool IsEnabled(OrchardVNext.Logging.LogLevel x) {
            return true;
        }
    }

    public class TestLoggerFactory : ILoggerFactory {

        public ILogger Create(string name) {
            return new TestLogger(name, true);
        }

        public void AddProvider(ILoggerProvider provider) {
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

        public IDisposable BeginScope(object state) {
            _scope = state;


            return NullDisposable.Instance;
        }

        public void Write(Microsoft.Framework.Logging.LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter) {
           Logger.Information(
               string.Format("LogLevel: {0}, EventId: {1}, State: {2}, Exception: {3}, Formatter: {4}, LoggerName: {5}, Scope: {6}",
                logLevel, eventId, state, exception, formatter, _name, _scope
            ));
        }

        public bool IsEnabled(Microsoft.Framework.Logging.LogLevel logLevel) {
            return _enabled;
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
