using System;
using Glimpse.Agent;
using Glimpse.Agent.Messages;
using Microsoft.Extensions.DiagnosticAdapter;
using Microsoft.Extensions.Logging;
using Orchard.Glimpse.Inspectors.Ado.Proxies;

namespace Orchard.Glimpse.Inspectors
{
    public partial class OrchardWebDiagnosticsInspector
    {
        [DiagnosticName("Orchard.Data.BeforeExecuteCommand")]
        public void OnBeforeExecuteCommand(IDbCommand command, string executeMethod, long timestamp, bool isAsync)
        {
            var message = new BeforeExecuteCommandMessage
            {
                CommandMethod = executeMethod,
                CommandIsAsync = isAsync,
                CommandText = command.CommandText,
                //CommandType = command.CommandType,
                //CommandParameters = command.Parameters,
                CommandStartTime = DateTime.FromBinary(timestamp)
            };
            
            _broker.BeginLogicalOperation(message, message.CommandStartTime);
            _broker.SendMessage(message);
        }

        [DiagnosticName("Orchard.Data.AfterExecuteCommand")]
        public void OnAfterExecuteCommand(IDbCommand command, string executeMethod, bool isAsync)
        {
            var timing = _broker.EndLogicalOperation<BeforeExecuteCommandMessage>();
            if (timing != null)
            {
                var message = new AfterExecuteCommandMessage
                {
                    CommandHadException = false,
                    CommandDuration = timing.Elapsed,
                    CommandEndTime = timing.End,
                    CommandOffset = timing.Offset
                };

                _broker.SendMessage(message);
            }
            else
            {
                _logger.LogCritical("OnAfterExecuteCommand: Couldn't publish `AfterExecuteCommandMessage` as `BeforeExecuteCommandMessage` wasn't found in stack");
            }
        }

        [DiagnosticName("Orchard.Data.CommandExecutionError")]
        public void OnAfterExecuteCommand(IDbCommand command, string executeMethod, bool isAsync, Exception exception)
        {
            var timing = _broker.EndLogicalOperation<BeforeExecuteCommandMessage>();
            if (timing != null)
            {
                var message = new AfterExecuteCommandExceptionMessage
                {
                    //Exception = exception,
                    CommandHadException = true,
                    CommandDuration = timing.Elapsed,
                    CommandEndTime = timing.End,
                    CommandOffset = timing.Offset
                };

                _broker.SendMessage(message);
            }
            else
            {
                _logger.LogCritical("OnAfterExecuteCommand: Couldn't publish `AfterExecuteCommandExceptionMessage` as `BeforeExecuteCommandMessage` wasn't found in stack");
            }
        }
    }
}
