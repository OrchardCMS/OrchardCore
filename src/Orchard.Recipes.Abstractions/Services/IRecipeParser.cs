using Microsoft.Extensions.FileProviders;
using Orchard.Recipes.Models;
using System;

namespace Orchard.Recipes.Services
{
    public interface IRecipeParser
    {
        RecipeDescriptor ParseRecipe(IFileInfo recipeFile);
        void ProcessRecipe(
            IFileInfo recipeFile,
            Action<RecipeDescriptor, RecipeStepDescriptor> action);
    }
}