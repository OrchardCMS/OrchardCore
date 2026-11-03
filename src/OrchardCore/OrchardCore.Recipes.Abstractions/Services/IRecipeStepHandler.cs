using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// An implementation of this interface will be used every time a recipe step is processed.
/// Each implementation is responsible for processing only the steps that it targets.
/// </summary>
public interface IRecipeStepHandler
{
    /// <summary>
    /// Processes a recipe step.
    /// </summary>
    /// <param name="context">The context object representing the processed step.</param>
    Task ExecuteAsync(RecipeExecutionContext context);

    /// <summary>
    /// Gets the names of the steps that this handler processes, which are not case-sensitive. A recipe is validated
    /// with them without executing it. When this is <see langword="null"/>, a step that no other handler declares
    /// can't be reported as unknown, since this handler may process it.
    /// </summary>
    IReadOnlyCollection<string> StepNames => null;
}
