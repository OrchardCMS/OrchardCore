using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Models;

namespace Orchard.Environment.Recipes.Services
{
    /// <summary>
    /// Provides information about the result of recipe execution.
    /// </summary>
    public interface IRecipeResultAccessor : IDependency
    {
        RecipeResult GetResult(string executionId);
    }
}