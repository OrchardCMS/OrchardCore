using Orchard.Recipes.Models;
using System.Threading.Tasks;

namespace Orchard.Recipes.Services
{
    /// <summary>
    /// Provides information about the result of recipe execution.
    /// </summary>
    public interface IRecipeResultAccessor
    {
        Task<RecipeResult> GetResultAsync(string executionId);
    }
}