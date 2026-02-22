using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Base class for recipe step handlers that handle a specific step name.
/// </summary>
/// <remarks>
/// This class is obsolete. Use <see cref="RecipeDeploymentStep{TModel}"/> or <see cref="RecipeImportStep{TModel}"/> instead,
/// which provide unified support for schema definition, recipe import, and deployment export.
/// </remarks>
[Obsolete($"Use {nameof(RecipeDeploymentStep<object>)} or {nameof(RecipeImportStep<object>)} instead. This class will be removed in a future version.", false)]
#pragma warning disable CS0618 // Type or member is obsolete - required for backwards compatibility
public abstract class NamedRecipeStepHandler : IRecipeStepHandler
#pragma warning restore CS0618
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
