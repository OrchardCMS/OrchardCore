using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Filters recipes based on context (e.g., tenant, user role, etc.).
/// </summary>
public interface IRecipeFilter
{
    /// <summary>
    /// Gets the order in which this filter runs. Lower values run first.
    /// </summary>
    int Order => 0;

    /// <summary>
    /// Determines whether the specified recipe should be included.
    /// </summary>
    /// <param name="recipe">The recipe descriptor to evaluate.</param>
    /// <returns><c>true</c> if the recipe should be included; otherwise, <c>false</c>.</returns>
    ValueTask<bool> ShouldIncludeAsync(RecipeDescriptor recipe);
}
