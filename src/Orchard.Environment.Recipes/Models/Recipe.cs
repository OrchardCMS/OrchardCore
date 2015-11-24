using System;
using System.Collections.Generic;

namespace Orchard.Environment.Recipes.Models
{
    public class Recipe
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string WebSite { get; set; }
        public string Version { get; set; }
        public bool IsSetupRecipe { get; set; }
        public DateTime? ExportUtc { get; set; }
        public string Category { get; set; }
        public string Tags { get; set; }
        public IEnumerable<RecipeStep> RecipeSteps { get; set; }
    }
}