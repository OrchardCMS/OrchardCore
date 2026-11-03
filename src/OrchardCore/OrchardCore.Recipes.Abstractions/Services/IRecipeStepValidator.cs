using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Optionally checks the properties of a recipe step without applying it, next to the handler of the step. A step
/// without a validator is only checked for a handler and for its file scripts. Register an implementation with
/// <see cref="ServiceCollectionExtensions.AddRecipeStepValidator{TImplementation}(Microsoft.Extensions.DependencyInjection.IServiceCollection)"/>.
/// </summary>
/// <remarks>
/// An implementation must not change anything or evaluate scripts, and has to report what the handler of the step
/// would really do with a value. Use <see cref="RecipeValidationContext.IsScript(string)"/> to skip a value only known
/// at execution.
/// </remarks>
public interface IRecipeStepValidator
{
    /// <summary>
    /// Gets the name of the validated step, which is not case-sensitive, like the name of a step handler.
    /// </summary>
    string StepName { get; }

    /// <summary>
    /// Validates the current step of the context, and adds its findings to it.
    /// </summary>
    /// <param name="context">The context of the validation.</param>
    Task ValidateAsync(RecipeValidationContext context);
}
