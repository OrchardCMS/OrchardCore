using Microsoft.Extensions.FileProviders;
using Orchard.Recipes.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Orchard.Recipes.Services
{
    public interface IRecipeParser
    {
        RecipeDescriptor ParseRecipe(Stream recipeStream);
        Task ProcessRecipeAsync(
            Stream recipeStream,
            Func<RecipeDescriptor, RecipeStepDescriptor, Task> action);
    }
}