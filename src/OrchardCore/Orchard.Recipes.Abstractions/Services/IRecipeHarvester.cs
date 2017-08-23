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

    }
}