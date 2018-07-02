using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.Recipes.ViewModels
{
    public class RecipeViewModel
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string[] Tags { get; set; }
        public string Description { get; set; }
        public string BasePath { get; set; }
        public string Feature { get; set; }
    }
}
