using Microsoft.Extensions.FileProviders;
using Orchard.Recipes.Models;
using System;
using System.IO;

namespace Orchard.Recipes.Services
{
    public interface IRecipeParser
    {
        RecipeDescriptor ParseRecipe(Stream recipeStream);
        void ProcessRecipe(
            Stream recipeStream,
            Action<RecipeDescriptor, RecipeStepDescriptor> action);
    }
}