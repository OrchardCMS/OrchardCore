using System.Threading.Tasks;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services
{
    public interface IRecipeExecutor
    {
        Task<string> ExecuteAsync(string executionId, RecipeDescriptor recipeDescriptor, object environment);
    }
}