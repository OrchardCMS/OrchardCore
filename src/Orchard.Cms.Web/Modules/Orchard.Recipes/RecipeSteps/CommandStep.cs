using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Commands;
using Orchard.Environment.Commands.Parameters;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.RecipeSteps
{
    public class CommandStep : RecipeExecutionStep
    {
        private readonly ICommandManager _commandManager;
        private readonly ICommandParser _commandParser;
        private readonly ICommandParametersParser _commandParameterParser;

        public CommandStep(ICommandManager commandManager,
            ICommandParser commandParser,
            ICommandParametersParser commandParameterParser,
            ILoggerFactory logger,
            IStringLocalizer<CommandStep> localizer) : base(logger, localizer)
        {

            _commandManager = commandManager;
            _commandParser = commandParser;
            _commandParameterParser = commandParameterParser;
        }

        public override string Name { get { return "Command"; } }

        public override Task ExecuteAsync(RecipeExecutionContext context)
        {
            var step = context.RecipeStep.Step.ToObject<InternalStep>();

            foreach(var command in step.Commands)
            {
                Logger.LogInformation("Executing command: {0}", command);

                var commandParameters = _commandParameterParser.Parse(_commandParser.Parse(command));

                _commandManager.ExecuteAsync(commandParameters);

                Logger.LogInformation("Executed command: {0}", command);
            }

            return Task.CompletedTask;
        }

        private class InternalStep
        {
            public string[] Commands { get; set; }
        }
    }
}
