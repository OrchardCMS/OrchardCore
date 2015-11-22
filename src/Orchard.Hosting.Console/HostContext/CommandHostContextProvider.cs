using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Commands;
using Orchard.Environment.Shell;
using System;
using System.Linq;

namespace Orchard.Hosting.HostContext
{
    public class CommandHostContextProvider : ICommandHostContextProvider
    {
        private readonly string[] _args;
        private readonly IServiceProvider _serviceProvider;
        private readonly OrchardConsoleLogger _logger;

        public CommandHostContextProvider(
            IServiceProvider serviceProvider,
            OrchardConsoleLogger logger,
            string[] args)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _args = args;
        }

        public CommandHostContext CreateContext()
        {
            var context = new CommandHostContext { RetryResult = CommandReturnCodes.Retry };

            _logger.LogInfo("Initializing Orchard session.");
            Initialize(context);
            return context;
        }

        public void Shutdown(CommandHostContext context)
        {
            _logger.LogInfo("Shutting down Orchard session");
        }

        private void Initialize(CommandHostContext context)
        {
            context.Arguments = new OrchardParametersParser().Parse(new CommandParametersParser().Parse(_args));
            context.CommandHost = new CommandHostAgent(
                _serviceProvider.GetService<IOrchardHost>(),
                _serviceProvider.GetService<IShellSettingsManager>()
                );

            // Perform some argument validation and display usage if something is incorrect
            context.DisplayUsageHelp = context.Arguments.Switches.ContainsKey("?");
            if (context.DisplayUsageHelp)
                return;

            context.DisplayUsageHelp = (context.Arguments.Arguments.Any() && context.Arguments.ResponseFiles.Any());
            if (context.DisplayUsageHelp)
            {
                _logger.LogError("Incorrect syntax: Response files cannot be used in conjunction with commands");
                return;
            }

            context.StartSessionResult = CommandReturnCodes.Ok;
        }
    }
}