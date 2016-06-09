using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services
{
    public interface IRecipeStepQueue
    {
        void Enqueue(string executionId, RecipeStep step);
        RecipeStep Dequeue(string executionId);
    }
}