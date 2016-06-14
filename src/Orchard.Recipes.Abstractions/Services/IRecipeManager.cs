using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services
{
    public interface IRecipeManager
    {
        string Execute(RecipeDescriptor recipe);
    }
}