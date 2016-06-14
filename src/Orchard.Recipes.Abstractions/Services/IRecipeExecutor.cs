using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services
{
    public interface IRecipeExecutor
    {
        string Execute(RecipeDescriptor recipe);
    }
}