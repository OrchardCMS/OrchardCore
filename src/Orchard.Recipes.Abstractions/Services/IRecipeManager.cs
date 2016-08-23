using Orchard.Recipes.Models;
using System.Threading.Tasks;

namespace Orchard.Recipes.Services
{
    public interface IRecipeManager
    {
        Task<string> ExecuteAsync(RecipeDescriptor recipeDescriptor);
        Task<string> ExecuteAsync(string executionId, RecipeDescriptor recipeDescriptor);
    }
}