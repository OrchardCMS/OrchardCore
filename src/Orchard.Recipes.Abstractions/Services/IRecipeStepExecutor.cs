using Orchard.Recipes.Models;
using System.Threading.Tasks;

namespace Orchard.Recipes.Services
{
    public interface IRecipeStepExecutor
    {
        Task ExecuteAsync(string executionId, RecipeStepDescriptor recipeStep);
    }
}