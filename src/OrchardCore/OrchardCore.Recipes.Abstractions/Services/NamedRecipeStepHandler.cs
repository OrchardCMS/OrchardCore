using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

public abstract class NamedRecipeStepHandler : IRecipeStepHandler
{
    /// <summary>
    /// The name of the recipe step.
    /// </summary>
    protected readonly string StepName;

    protected NamedRecipeStepHandler(string stepName)
    {
        StepName = stepName;
    }

    public Task ExecuteAsync(RecipeExecutionContext context)
    {
        if (!string.Equals(context.Name, StepName, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        return HandleAsync(context);
    }

    protected abstract Task HandleAsync(RecipeExecutionContext context);
}

public abstract class NamedRecipeStepHandler<T> : ISchemaAwareRecipeStepHandler
    where T : class, new()
{
    private RecipeSchema _schema;

    /// <summary>
    /// The name of the recipe step.
    /// </summary>
    protected readonly string StepName;

    protected NamedRecipeStepHandler(string stepName)
    {
        StepName = stepName;
    }

    public Task ExecuteAsync(RecipeExecutionContext context)
    {
        if (!string.Equals(context.Name, StepName, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        return HandleAsync(context);
    }

    public RecipeSchema GetSchema()
    {
        return _schema ??= new RecipeSchema
        {
            Name = StepName,
            Data = ConvertToJsonObject(new T()),
        };
    }

    protected abstract Task HandleAsync(RecipeExecutionContext context);

    private static JsonObject ConvertToJsonObject(T obj)
    {
        var jsonString = JsonSerializer.Serialize(obj);

        // Parse the JSON string into a JsonObject
        var jsonNode = JsonNode.Parse(jsonString);

        // Cast to JsonObject (if possible)
        return jsonNode as JsonObject ?? throw new InvalidOperationException("The object could not be converted to JsonObject.");
    }
}
