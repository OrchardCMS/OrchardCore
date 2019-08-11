using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Commands;
using OrchardCore.Environment.Commands.Parameters;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes.RecipeSteps
{
    /// <summary>
    /// This recipe step executes a set of commands.
    /// </summary>
    public class CommandStep : IRecipeStepHandler
    {
        private readonly ICommandManager _commandManager;
        private readonly ICommandParser _commandParser;
        private readonly ICommandParametersParser _commandParameterParser;

        public CommandStep(ICommandManager commandManager,
            ICommandParser commandParser,
            ICommandParametersParser commandParameterParser,
            ILogger<CommandStep> logger)
        {
            _commandManager = commandManager;
            _commandParser = commandParser;
            _commandParameterParser = commandParameterParser;
            Logger = logger;
        }

        private ILogger Logger { get; set; }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        { 
            if (!String.Equals(context.Name, "Command", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var step = context.Step.ToObject<CommandStepModel>();

            foreach(var command in step.Commands)
            {
                using (var output = new StringWriter())
                {
                    Logger.LogInformation("Executing command: {Command}", command);
                    var commandParameters = _commandParameterParser.Parse(_commandParser.Parse(command));
                    commandParameters.Output = output;
                    await _commandManager.ExecuteAsync(commandParameters);
                    Logger.LogInformation("Command executed with output: {CommandOutput}", output);
                }
                Logger.LogInformation("Executed command: {Command}", command);
            }
        }

        private class CommandStepModel
        {
            public string[] Commands { get; set; }
        }
    }
}
