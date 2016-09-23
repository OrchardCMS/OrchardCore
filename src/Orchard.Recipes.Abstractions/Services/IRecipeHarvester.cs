using Orchard.Recipes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Recipes.Services
{
    public interface IRecipeHarvester
    {
        /// <summary>
        /// Returns a collection of all recipes.
        /// </summary>
        Task<IEnumerable<RecipeDescriptor>> HarvestRecipesAsync();

        /// <summary>
        /// Returns a collection of all recipes found in the specified extension.
        /// </summary>
        Task<IEnumerable<RecipeDescriptor>> HarvestRecipesAsync(string extensionId);
    }

    public static class RecipeHarvesterExtensions
    {
        public static RecipeDescriptor GetRecipeByName(this IEnumerable<RecipeDescriptor> recipes, string recipeName)
        {
            return recipes.FirstOrDefault(r => r.Name.Equals(recipeName, StringComparison.OrdinalIgnoreCase));
        }
    }
}