using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystem;
using Orchard.Recipes.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Recipes.Services
{
    public class RecipeHarvester : IRecipeHarvester
    {
        private readonly IExtensionManager _extensionManager;
        private readonly IOrchardFileSystem _fileSystem;
        private readonly IRecipeParser _recipeParser;
        private readonly Matcher _fileMatcher;

        public RecipeHarvester(IExtensionManager extensionManager,
            IOrchardFileSystem fileSystem,
            IRecipeParser recipeParser,
            IOptions<RecipeHarvestingOptions> recipeOptions,
            IStringLocalizer<RecipeHarvester> localizer,
            ILogger<RecipeHarvester> logger) {
            _extensionManager = extensionManager;
            _fileSystem = fileSystem;
            _recipeParser = recipeParser;

            _fileMatcher = new Matcher(System.StringComparison.OrdinalIgnoreCase);
            _fileMatcher.AddIncludePatterns(recipeOptions.Value.RecipeFileExtensions);

            T = localizer;
            Logger = logger;
        }

        public IStringLocalizer T { get; set; }
        public ILogger Logger { get; set; }

        public async Task<IEnumerable<RecipeDescriptor>> HarvestRecipesAsync()
        {
            return await _extensionManager.AvailableExtensions().InvokeAsync(async descriptor => {
                return await HarvestRecipesAsync(descriptor);
            }, Logger);
        }

        public async Task<IEnumerable<RecipeDescriptor>> HarvestRecipesAsync(string extensionId)
        {
            var extension = _extensionManager.GetExtension(extensionId);
            if (extension != null)
            {
                return await HarvestRecipesAsync(extension);
            }

            Logger.LogError(T["Could not discover recipes because extension '{0}' was not found.", extensionId]);
            return Enumerable.Empty<RecipeDescriptor>();
        }

        private async Task<IEnumerable<RecipeDescriptor>> HarvestRecipesAsync(ExtensionDescriptor extension)
        {
            var recipeLocation = _fileSystem.Combine(extension.Location, extension.Id, "Recipes");
            var recipeFiles = _fileSystem.ListFiles(recipeLocation, _fileMatcher);

            return await recipeFiles.InvokeAsync(async recipeFile => {
                return await Task.FromResult(_recipeParser.ParseRecipe(recipeFile));
            }, Logger);
        }
    }
}
