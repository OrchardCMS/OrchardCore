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
        private readonly IEnumerable<IRecipeParser> _recipeParsers;
        private readonly IOptions<RecipeHarvestingOptions> _recipeOptions;

        public RecipeHarvester(IExtensionManager extensionManager,
            IOrchardFileSystem fileSystem,
            IEnumerable<IRecipeParser> recipeParsers,
            IOptions<RecipeHarvestingOptions> recipeOptions,
            IStringLocalizer<RecipeHarvester> localizer,
            ILogger<RecipeHarvester> logger) {
            _extensionManager = extensionManager;
            _fileSystem = fileSystem;
            _recipeParsers = recipeParsers;
            _recipeOptions = recipeOptions;

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
            var descriptor = _extensionManager.GetExtension(extensionId);
            if (descriptor != null)
            {
                return await HarvestRecipesAsync(descriptor);
            }

            Logger.LogError(T["Could not discover recipes because extension '{0}' was not found.", extensionId]);
            return Enumerable.Empty<RecipeDescriptor>();
        }

        private async Task<IEnumerable<RecipeDescriptor>> HarvestRecipesAsync(ExtensionDescriptor extension)
        {
            var recipeLocation = _fileSystem.Combine(extension.Location, extension.Id, "Recipes");

            var recipeOptions = _recipeOptions.Value;

            List<RecipeDescriptor> recipeDescriptors = new List<RecipeDescriptor>();

            foreach(var recipeFileExtension in recipeOptions.RecipeFileExtensions)
            {
                var fileMatcher = new Matcher(System.StringComparison.OrdinalIgnoreCase);
                fileMatcher.AddInclude(recipeFileExtension.Key);

                var recipeFiles = _fileSystem.ListFiles(recipeLocation, fileMatcher);

                recipeDescriptors.AddRange(await recipeFiles.InvokeAsync(recipeFile => {
                    var recipeParser = _recipeParsers.First(x => x.GetType() == recipeFileExtension.Value);
                    using (var stream = recipeFile.CreateReadStream()) {
                        var recipe = recipeParser.ParseRecipe(stream);
                        recipe.RecipeFileInfo = recipeFile;
                        return Task.FromResult(recipe);
                    }
                }, Logger));
            }

            return recipeDescriptors;
        }
    }
}
