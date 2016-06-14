using System.Collections.Generic;

namespace Orchard.Recipes.Services
{
    public class RecipeHarvestingOptions
    {
        public IList<string> RecipeFileExtensions { get; }
            = new List<string>();
    }
}