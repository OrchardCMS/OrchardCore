using Orchard.Recipes.Models;
using System.Threading.Tasks;

namespace Orchard.Recipes.Services
{
    public interface IRecipeStepQueue
    {
        Task EnqueueAsync(string executionId, RecipeStepDescriptor step);
        Task<RecipeStepDescriptor> DequeueAsync(string executionId);
    }
}