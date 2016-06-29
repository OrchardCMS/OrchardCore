using Orchard.Events;
using Orchard.Recipes.Models;
using System.Threading.Tasks;

namespace Orchard.Recipes.Events
{
    public interface IRecipeExecuteEventHandler : IEventHandler
    {
        Task ExecutionStartAsync(string executionId, RecipeDescriptor recipe);
        Task RecipeStepExecutingAsync(string executionId, RecipeContext context);
        Task RecipeStepExecutedAsync(string executionId, RecipeContext context);
        Task ExecutionCompleteAsync(string executionId);
        Task ExecutionFailedAsync(string executionId);
    }
}