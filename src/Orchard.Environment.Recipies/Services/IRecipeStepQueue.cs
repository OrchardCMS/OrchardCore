using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Models;

namespace Orchard.Environment.Recipes.Services
{
    public interface IRecipeStepQueue : ISingletonDependency
    {
        void Enqueue(string executionId, RecipeStep step);
        RecipeStep Dequeue(string executionId);
    }
}