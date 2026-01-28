using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Base class for implementing recipe/deployment steps with strongly-typed models.
/// </summary>
/// <typeparam name="TModel">The strongly-typed model for the step data.</typeparam>
public abstract class RecipeDeploymentStep<TModel> : IRecipeDeploymentStep
    where TModel : class, new()
{
    private JsonSchema _schema;

    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public JsonSchema Schema => _schema ??= BuildSchema();

    /// <summary>
    /// Builds the JSON Schema for this step.
    /// </summary>
    protected abstract JsonSchema BuildSchema();

    /// <inheritdoc />
    public async Task ExecuteAsync(RecipeExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!string.Equals(context.Name, Name, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var model = DeserializeStep(context.Step);

        await ImportAsync(model, context);
    }

    /// <inheritdoc />
    public async Task ExportAsync(RecipeExportContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var model = await BuildExportModelAsync(context);
        if (model is null)
        {
            return;
        }

        var stepData = SerializeStep(model);

        // Ensure the name property is set.
        stepData["name"] = Name;

        context.AddStep(stepData);
    }

    /// <summary>
    /// Imports data from a recipe step.
    /// </summary>
    /// <param name="model">The deserialized step model.</param>
    /// <param name="context">The execution context.</param>
    protected abstract Task ImportAsync(TModel model, RecipeExecutionContext context);

    /// <summary>
    /// Builds the model for export. Return null to skip export.
    /// </summary>
    /// <param name="context">The export context.</param>
    /// <returns>The model to export, or null to skip.</returns>
    protected abstract Task<TModel> BuildExportModelAsync(RecipeExportContext context);

    /// <summary>
    /// Deserializes the step JSON to the model type.
    /// </summary>
    protected virtual TModel DeserializeStep(JsonObject step)
    {
        return step.Deserialize<TModel>(JOptions.Default) ?? new TModel();
    }

    /// <summary>
    /// Serializes the model to step JSON.
    /// </summary>
    protected virtual JsonObject SerializeStep(TModel model)
    {
        var json = JsonSerializer.SerializeToNode(model, JOptions.Default);

        var result = json?.AsObject() ?? new JsonObject();

        return result;
    }
}
