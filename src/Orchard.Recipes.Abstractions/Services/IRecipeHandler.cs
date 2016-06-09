using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services
{
    public interface IRecipeHandler
    {
        void ExecuteRecipeStep(RecipeContext recipeContext);
    }
}