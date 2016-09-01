using Orchard.Recipes.Models;
using System.Threading.Tasks;

namespace Orchard.Recipes.Services
{
    public interface IRecipeExecutor
    {
        Task<string> ExecuteAsync(string executionId, RecipeDescriptor recipeDescriptor);
    }
}