using System.Text.Json.Nodes;
using Cysharp.Text;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Commands;
using OrchardCore.Environment.Commands.Parameters;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes.RecipeSteps;

/// <summary>
/// This recipe step executes a set of commands.
/// </summary>
public sealed class CommandStep : NamedRecipeStepHandler
{
    private readonly ICommandManager _commandManager;
    private readonly ICommandParser _commandParser;
    private readonly ICommandParametersParser _commandParameterParser;
    private readonly ILogger _logger;

    public CommandStep(ICommandManager commandManager,
        ICommandParser commandParser,
        ICommandParametersParser commandParameterParser,
        ILogger<CommandStep> logger)
        : base("Command")
    {
        _commandManager = commandManager;
        _commandParser = commandParser;
        _commandParameterParser = commandParameterParser;
        _logger = logger;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var step = context.Step.ToObject<CommandStepModel>();

        foreach (var command in step.Commands)
        {
            await using (var output = new ZStringWriter())
            {
                _logger.LogInformation("Executing command: {Command}", command);

                var commandParameters = _commandParameterParser.Parse(_commandParser.Parse(command));
                commandParameters.Output = output;
                await _commandManager.ExecuteAsync(commandParameters);

                _logger.LogInformation("Command executed with output: {CommandOutput}", output);
            }

            _logger.LogInformation("Executed command: {Command}", command);
        }
    }

    private sealed class CommandStepModel
    {
        public string[] Commands { get; set; }
    }
}
