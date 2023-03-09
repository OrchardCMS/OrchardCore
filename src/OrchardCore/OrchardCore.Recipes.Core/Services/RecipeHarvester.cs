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

        /// <inheritdoc/>
        public virtual Task<IEnumerable<RecipeDescriptor>> HarvestRecipesAsync()
            => _extensionManager.GetExtensions().InvokeAsync(HarvestRecipes, _logger);

        /// <summary>
        /// Returns a list of recipes for a content path.
        /// </summary>
        /// <param name="path">A path string relative to the content root of the application.</param>
        /// <returns>The list of <see cref="RecipeDescriptor"/> instances.</returns>
        protected async Task<IEnumerable<RecipeDescriptor>> HarvestRecipesAsync(string path)
        {
            var recipeDescriptors = new List<RecipeDescriptor>();

            var recipeFiles = _hostingEnvironment.ContentRootFileProvider.GetDirectoryContents(path)
                .Where(f => !f.IsDirectory && f.Name.EndsWith(RecipesConstants.RecipeExtension, StringComparison.Ordinal));

            foreach (var recipeFile in recipeFiles)
            {
                var recipeDescriptor = await _recipeReader.GetRecipeDescriptorAsync(path, recipeFile, _hostingEnvironment.ContentRootFileProvider);

                recipeDescriptors.Add(recipeDescriptor);
            }

            return recipeDescriptors;
        }

        private Task<IEnumerable<RecipeDescriptor>> HarvestRecipes(IExtensionInfo extension)
        {
            var folderSubPath = PathExtensions.Combine(extension.SubPath, RecipesConstants.RecipesFolderName);

            return HarvestRecipesAsync(folderSubPath);
        }
    }
}
