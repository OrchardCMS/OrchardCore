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
}