using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services
{
    /// <summary>
    /// Provides information about the result of recipe execution.
    /// </summary>
    public interface IRecipeResultAccessor
    {
        RecipeResult GetResult(string executionId);
    }
}