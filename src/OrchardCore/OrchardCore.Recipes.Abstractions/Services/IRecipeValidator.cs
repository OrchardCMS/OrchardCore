using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Checks what can be checked in a recipe without applying anything: it parses the recipe, looks for a handler of
/// each step, and runs the <see cref="IRecipeStepValidator"/> of each step. It doesn't evaluate scripts.
/// </summary>
public interface IRecipeValidator
{
    /// <summary>
    /// Validates a recipe without executing it. A valid result doesn't guarantee that the execution succeeds, since
    /// a step can still fail on its data.
    /// </summary>
    /// <param name="recipe">The recipe, whose file provider and base path resolve the files it refers to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<RecipeValidationResult> ValidateAsync(RecipeDescriptor recipe, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates the invalid result of a recipe that couldn't be read at all, for instance from an invalid package.
    /// </summary>
    /// <param name="code">The stable code of the finding.</param>
    /// <param name="message">The message of the finding.</param>
    RecipeValidationResult CreateFailure(string code, string message);
}
