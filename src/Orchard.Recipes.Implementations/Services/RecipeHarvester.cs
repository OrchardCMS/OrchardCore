using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Recipes.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace Orchard.Recipes.Services
{
    public class RecipeHarvester : IRecipeHarvester
    {
        private readonly IExtensionManager _extensionManager;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IEnumerable<IRecipeParser> _recipeParsers;
        private readonly IOptions<RecipeHarvestingOptions> _recipeOptions;

        public RecipeHarvester(IExtensionManager extensionManager,
            IHostingEnvironment hostingEnvironment,
            IEnumerable<IRecipeParser> recipeParsers,
            IOptions<RecipeHarvestingOptions> recipeOptions,
            IStringLocalizer<RecipeHarvester> localizer,
            ILogger<RecipeHarvester> logger) {
            _extensionManager = extensionManager;
            _hostingEnvironment = hostingEnvironment;
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
            var folderSubPath = Path.Combine(extension.Location, extension.Id, "Recipes");
            var recipeContainerFileInfo = _hostingEnvironment
                .ContentRootFileProvider
                .GetFileInfo(folderSubPath);

            var recipeOptions = _recipeOptions.Value;

            List<RecipeDescriptor> recipeDescriptors = new List<RecipeDescriptor>();

            foreach(var recipeFileExtension in recipeOptions.RecipeFileExtensions)
            {
                var matcher = new Matcher(System.StringComparison.OrdinalIgnoreCase);
                matcher.AddInclude(recipeFileExtension.Key);

                var matches = matcher
                    .Execute(new DirectoryInfoWrapper(new DirectoryInfo(recipeContainerFileInfo.PhysicalPath)))
                    .Files;

                if (matches.Any())
                {
                    var result = matches
                        .Select(match => _hostingEnvironment
                            .ContentRootFileProvider
                            .GetFileInfo(Path.Combine(folderSubPath, match.Path))).ToArray();

                    recipeDescriptors.AddRange(await result.InvokeAsync(recipeFile =>
                    {
                        var recipeParser = _recipeParsers.First(x => x.GetType() == recipeFileExtension.Value);
                        using (var stream = recipeFile.CreateReadStream())
                        {
                            var recipe = recipeParser.ParseRecipe(stream);
                            recipe.RecipeFileName = recipeFile.Name;
                            recipe.RecipeFileProvider = new PhysicalFileProvider(Path.GetDirectoryName(recipeFile.PhysicalPath));
                            return Task.FromResult(recipe);
                        }
                    }, Logger));
                }
            }

            return recipeDescriptors;
        }
    }
}
