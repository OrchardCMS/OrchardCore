namespace OrchardCore.Recipes.Services;

/// <summary>
/// Base class for import-only recipe steps that do not support deployment export.
/// </summary>
/// <typeparam name="TModel">The strongly-typed model for the step data.</typeparam>
public abstract class RecipeImportStep<TModel> : RecipeDeploymentStep<TModel>
    where TModel : class, new()
{
    /// <inheritdoc />
    protected sealed override Task<TModel> BuildExportModelAsync(RecipeExportContext context)
        => Task.FromResult<TModel>(null);
}
