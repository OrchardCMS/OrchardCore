using System;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Recipes.Models
{
    public class RecipeDescriptor
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string WebSite { get; set; }
        public string Version { get; set; }
        public bool IsSetupRecipe { get; set; }
        public DateTime? ExportUtc { get; set; }
        public string[] Categories { get; set; }
        public string[] Tags { get; set; }
        public bool RequireNewScope { get; set; } = true;

        /// <summary>
        /// The path of the recipe file for the <see cref="RecipeDescriptor.FileProvider"/> property
        /// </summary>
        public string BasePath { get; set; }

        public IFileInfo RecipeFileInfo { get; set; }
        public IFileProvider FileProvider { get; set; }
    }
}
