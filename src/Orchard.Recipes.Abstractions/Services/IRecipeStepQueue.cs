using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services
{
    public interface IRecipeStepQueue
    {
        void Enqueue(string executionId, RecipeStepDescriptor step);
        RecipeStepDescriptor Dequeue(string executionId);
    }
}