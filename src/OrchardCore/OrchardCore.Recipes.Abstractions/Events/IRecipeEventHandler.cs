using System.Threading.Tasks;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Events
{
    /// <summary>
    /// Note: The recipe executor creates for each step a scope that may be based on a new shell if the features have changed,
    /// so an 'IRecipeEventHandler', that is also used in each step scope, can't be a tenant level service, otherwise it may
    /// be used in a shell container it doesn't belong, so it should be an application level transient or singleton service.
    /// </summary>
    public interface IRecipeEventHandler
    {
        Task RecipeExecutingAsync(string executionId, RecipeDescriptor descriptor);
        Task RecipeStepExecutingAsync(RecipeExecutionContext context);
        Task RecipeStepExecutedAsync(RecipeExecutionContext context);
        Task RecipeExecutedAsync(string executionId, RecipeDescriptor descriptor);
        Task ExecutionFailedAsync(string executionId, RecipeDescriptor descriptor);
    }
}
