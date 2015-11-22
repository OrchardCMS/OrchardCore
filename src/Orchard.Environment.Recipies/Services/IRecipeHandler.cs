using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Models;

namespace Orchard.Environment.Recipes.Services
{
    public interface IRecipeHandler : IDependency
    {
        void ExecuteRecipeStep(RecipeContext recipeContext);
    }
}