using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// An implementation of this interface will be used every time a recipe step is processed.
/// Each implementation is responsible for processing only the steps that it targets.
/// </summary>
/// <remarks>
/// This interface is obsolete. Use <see cref="IRecipeDeploymentStep"/> instead, which provides
/// unified support for schema definition, recipe import, and deployment export.
/// </remarks>
[Obsolete($"Use {nameof(IRecipeDeploymentStep)} instead. This interface will be removed in a future version.", false)]
public interface IRecipeStepHandler
{
    /// <summary>
    /// Processes a recipe step.
    /// </summary>
    /// <param name="context">The context object representing the processed step.</param>
    Task ExecuteAsync(RecipeExecutionContext context);
}
