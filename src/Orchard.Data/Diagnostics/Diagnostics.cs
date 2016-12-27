using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Data.Diagnostics
{
    internal static class Diagnostics
    {
        private const string NamePrefix = "Orchard.Data.";

        public const string BeforeExecuteCommand = NamePrefix + nameof(BeforeExecuteCommand);
        public const string AfterExecuteCommand = NamePrefix + nameof(AfterExecuteCommand);
        public const string CommandExecutionError = NamePrefix + nameof(CommandExecutionError);

        public const string DataReaderDisposing = NamePrefix + nameof(DataReaderDisposing);

        public static void WriteCommandBefore(
            this DiagnosticSource diagnosticSource,
            DbCommand command,
            string executeMethod,
            Guid instanceId,
            long startTimestamp,
            bool async)
        {
            //if (diagnosticSource.IsEnabled(BeforeExecuteCommand))
            //{
                diagnosticSource.Write(
                    BeforeExecuteCommand,
                    new DataDiagnosticSourceBeforeMessage
                    {
                        Command = command,
                        ExecuteMethod = executeMethod,
                        InstanceId = instanceId,
                        Timestamp = startTimestamp,
                        IsAsync = async
                    });
            //}
        }

        public static void WriteCommandAfter(
           this DiagnosticSource diagnosticSource,
           DbCommand command,
           string executeMethod,
           Guid instanceId,
           long startTimestamp,
           long currentTimestamp,
           bool async = false)
        {
            //if (diagnosticSource.IsEnabled(AfterExecuteCommand))
            //{
                diagnosticSource.Write(
                    AfterExecuteCommand,
                    new DataDiagnosticSourceAfterMessage
                    {
                        Command = command,
                        ExecuteMethod = executeMethod,
                        InstanceId = instanceId,
                        Timestamp = currentTimestamp,
                        Duration = currentTimestamp - startTimestamp,
                        IsAsync = async
                    });
            //}
        }

        public static void WriteCommandError(
            this DiagnosticSource diagnosticSource,
            DbCommand command,
            string executeMethod,
            Guid instanceId,
            long startTimestamp,
            long currentTimestamp,
            Exception exception,
            bool async)
        {
            //if (diagnosticSource.IsEnabled(CommandExecutionError))
            //{
                diagnosticSource.Write(
                    CommandExecutionError,
                    new DataDiagnosticSourceAfterMessage
                    {
                        Command = command,
                        ExecuteMethod = executeMethod,
                        InstanceId = instanceId,
                        Timestamp = currentTimestamp,
                        Duration = currentTimestamp - startTimestamp,
                        Exception = exception,
                        IsAsync = async
                    });
            //}
        }


        private class DataDiagnosticSourceBeforeMessage
        {
            public DbCommand Command { get; set; }
            public string ExecuteMethod { get; set; }
            public bool IsAsync { get; set; }
            public Guid InstanceId { get; set; }
            public long Timestamp { get; set; }
        }

        internal class DataDiagnosticSourceAfterMessage
        {
            public DbCommand Command { get; set; }
            public string ExecuteMethod { get; set; }
            public bool IsAsync { get; set; }
            public Guid InstanceId { get; set; }
            public long Timestamp { get; set; }
            public long Duration { get; set; }
            public Exception Exception { get; set; }
        }
    }
}