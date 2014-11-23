using OrchardVNext.Logging;
using System;

namespace OrchardVNext
{
    public static class Logger
    {
        public static void Debug(string value, params object[] args) {
            Console.WriteLine(value, args);
        }

        public static void Information(string value, params object[] args) {
            Console.WriteLine(value, args);
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

        public static bool IsEnabled(LogLevel x) {
            return true;
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
