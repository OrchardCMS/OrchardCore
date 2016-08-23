using Orchard.Recipes.Models;
using System.Threading.Tasks;

namespace Orchard.Recipes.Services
{
    public interface IRecipeExecutor
    {
        Task ExecuteAsync(string executionId, RecipeDescriptor recipeDescriptor);
    }
}