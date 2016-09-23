using System.Threading.Tasks;
using Orchard.Events;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Events
{
    public interface IRecipeEventHandler : IEventHandler
    {
        Task RecipeExecutingAsync(string executionId, RecipeDescriptor descriptor);
        Task RecipeStepExecutingAsync(string executionId, RecipeStepDescriptor context);
        Task RecipeStepExecutedAsync(string executionId, RecipeStepDescriptor context);
        Task RecipeExecutedAsync(string executionId, RecipeDescriptor descriptor);
        Task ExecutionFailedAsync(string executionId, RecipeDescriptor descriptor);
    }
}