using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Extensions;
using OrchardCore.Modules;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services
{
    public class RecipeHarvester : IRecipeHarvester
    {
        private readonly IRecipeReader _recipeReader;
        private readonly IExtensionManager _extensionManager;
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly ILogger _logger;

        public RecipeHarvester(
            IRecipeReader recipeReader,
            IExtensionManager extensionManager,
            IHostEnvironment hostingEnvironment,
            ILogger<RecipeHarvester> logger)
        {
            _recipeReader = recipeReader;
            _extensionManager = extensionManager;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        public virtual Task<IEnumerable<RecipeDescriptor>> HarvestRecipesAsync()
        {
            return _extensionManager.GetExtensions().InvokeAsync(HarvestRecipes, _logger);
        }

        private Task<IEnumerable<RecipeDescriptor>> HarvestRecipes(IExtensionInfo extension)
        {
            var folderSubPath = PathExtensions.Combine(extension.SubPath, "Recipes");
            return HarvestRecipesAsync(folderSubPath);
        }

        /// <summary>
        /// Returns a list of recipes for a content path.
        /// </summary>
        /// <param name="path">A path string relative to the content root of the application.</param>
        /// <returns>The list of <see cref="RecipeDescriptor"/> instances.</returns>
        protected Task<IEnumerable<RecipeDescriptor>> HarvestRecipesAsync(string path)
        {
            var recipeDescriptors = new List<RecipeDescriptor>();

            var recipeFiles = _hostingEnvironment.ContentRootFileProvider.GetDirectoryContents(path)
                .Where(x => !x.IsDirectory && x.Name.EndsWith(".recipe.json", StringComparison.Ordinal));

            recipeDescriptors.AddRange(recipeFiles.Select(recipeFile => _recipeReader.GetRecipeDescriptor(path, recipeFile, _hostingEnvironment.ContentRootFileProvider).Result));

            return Task.FromResult<IEnumerable<RecipeDescriptor>>(recipeDescriptors);
        }
    }
}
