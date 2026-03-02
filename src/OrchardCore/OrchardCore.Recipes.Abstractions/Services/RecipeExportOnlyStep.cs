using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Base class for export-only deployment steps that do not support recipe import.
/// </summary>
/// <typeparam name="TModel">The strongly-typed model for the step data.</typeparam>
public abstract class RecipeExportOnlyStep<TModel> : RecipeDeploymentStep<TModel>
    where TModel : class, new()
{
    /// <inheritdoc />
    protected sealed override Task ImportAsync(TModel model, RecipeExecutionContext context)
        => Task.CompletedTask;
}
