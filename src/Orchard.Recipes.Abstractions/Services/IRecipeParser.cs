using Microsoft.Extensions.FileProviders;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services
{
    public interface IRecipeParser
    {
        RecipeDescriptor ParseRecipe(IFileInfo recipeFile);
    }
}