using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Recipes.Models;
using Orchard.DependencyInjection;

namespace Orchard.Environment.Recipes.Services
{
    public interface IRecipeHarvester : IDependency
    {
        /// <summary>
        /// Returns a collection of all recipes.
        /// </summary>
        IEnumerable<Recipe> HarvestRecipes();

        /// <summary>
        /// Returns a collection of all recipes found in the specified extension.
        /// </summary>
        IEnumerable<Recipe> HarvestRecipes(string extensionId);
    }

    public static class RecipeHarvesterExtensions
    {
        public static Recipe GetRecipeByName(this IEnumerable<Recipe> recipes, string recipeName)
        {
            return recipes.FirstOrDefault(r => r.Name.Equals(recipeName, StringComparison.OrdinalIgnoreCase));
        }
    }
}