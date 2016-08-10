using System;
using System.Collections.Generic;

namespace Orchard.Recipes.Services
{
    public class RecipeHarvestingOptions
    {
        public IDictionary<string, Type> RecipeFileExtensions { get; }
            = new Dictionary<string, Type>();
    }
}