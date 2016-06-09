using Orchard.Recipes.Models;

namespace Orchard.Recipes.Events
{
    public interface IRecipeExecuteEventHandler
    {
        void ExecutionStart(string executionId, Recipe recipe);
        void RecipeStepExecuting(string executionId, RecipeContext context);
        void RecipeStepExecuted(string executionId, RecipeContext context);
        void ExecutionComplete(string executionId);
        void ExecutionFailed(string executionId);
    }
}