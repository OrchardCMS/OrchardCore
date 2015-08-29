using Microsoft.Framework.Logging;
using Orchard.Hosting.Console.Host;
using System;
using System.IO;
using System.Linq;

namespace Orchard.Hosting.Console.HostContext {
    public class CommandHostContextProvider : ICommandHostContextProvider {
        private readonly ILogger _logger;
        private readonly string[] _args;
        private TextWriter _output;
        private TextReader _input;

        public CommandHostContextProvider(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            string[] args) {
            _logger = loggerFactory.CreateLogger<CommandHostContextProvider>();
            _input = System.Console.In;
            _output = System.Console.Out;
            _args = args;
        }

        public CommandHostContext CreateContext() {
            var context = new CommandHostContext { RetryResult = CommandReturnCodes.Retry };
            Initialize(context);
            return context;
        }

        public void Shutdown(CommandHostContext context) {
        }

        private void Initialize(CommandHostContext context) {
            context.Arguments = new OrchardParametersParser().Parse(new CommandParametersParser().Parse(_args));

            // Perform some argument validation and display usage if something is incorrect
            context.DisplayUsageHelp = context.Arguments.Switches.ContainsKey("?");
            if (context.DisplayUsageHelp)
                return;

            context.DisplayUsageHelp = (context.Arguments.Arguments.Any() && context.Arguments.ResponseFiles.Any());
            if (context.DisplayUsageHelp) {
                _output.WriteLine("Incorrect syntax: Response files cannot be used in conjunction with commands");
                return;
            }

            context.StartSessionResult = CommandReturnCodes.Ok;
        }
    }
}
