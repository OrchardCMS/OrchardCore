using Orchard.Recipes.Models;
using System.Threading.Tasks;

namespace Orchard.Recipes.Services
{
    public interface IRecipeQueue
    {
        Task<string> EnqueueAsync(string executionId, RecipeDescriptor recipeDescriptor);
    }
}