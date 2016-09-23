using System.Threading.Tasks;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services
{
    public interface IRecipeHandler
    {
        Task ExecuteRecipeStepAsync(RecipeContext recipeContext);
    }
}