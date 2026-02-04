using Cysharp.Text;
using OrchardCore.Recipes.Schema;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Commands;
using OrchardCore.Environment.Commands.Parameters;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes.RecipeSteps;

/// <summary>
/// Unified recipe step for executing CLI commands.
/// </summary>
public sealed class UnifiedCommandStep : RecipeImportStep<UnifiedCommandStep.CommandStepModel>
{
    private readonly ICommandManager _commandManager;
    private readonly ICommandParser _commandParser;
    private readonly ICommandParametersParser _commandParameterParser;
    private readonly ILogger _logger;

    public UnifiedCommandStep(
        ICommandManager commandManager,
        ICommandParser commandParser,
        ICommandParametersParser commandParameterParser,
        ILogger<UnifiedCommandStep> logger)
    {
        _commandManager = commandManager;
        _commandParser = commandParser;
        _commandParameterParser = commandParameterParser;
        _logger = logger;
    }

    /// <inheritdoc />
    public override string Name => "Command";

    /// <inheritdoc />
    protected override RecipeStepSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title(Name)
            .Description("Executes CLI commands.")
            .Required("name", "Commands")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("Commands", new RecipeStepSchemaBuilder()
                    .TypeArray()
                    .Description("Array of CLI commands to execute.")
                    .Items(new RecipeStepSchemaBuilder().TypeString())))
            .AdditionalProperties(false)
            .Build();
    }

    /// <inheritdoc />
    protected override async Task ImportAsync(CommandStepModel model, RecipeExecutionContext context)
    {
        foreach (var command in model.Commands ?? [])
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

    /// <summary>
    /// Model for the Command step data.
    /// </summary>
    public sealed class CommandStepModel
    {
        public string[] Commands { get; set; }
    }
}
