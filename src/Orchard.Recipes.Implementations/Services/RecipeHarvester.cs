using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Orchard.Environment.Extensions;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services
{
    public class RecipeHarvester : IRecipeHarvester
    {
        private readonly IExtensionManager _extensionManager;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IOptions<RecipeHarvestingOptions> _recipeOptions;

        public RecipeHarvester(IExtensionManager extensionManager,
            IHostingEnvironment hostingEnvironment,
            IOptions<RecipeHarvestingOptions> recipeOptions,
            IStringLocalizer<RecipeHarvester> localizer,
            ILogger<RecipeHarvester> logger) {
            _extensionManager = extensionManager;
            _hostingEnvironment = hostingEnvironment;
            _recipeOptions = recipeOptions;

            T = localizer;
            Logger = logger;
        }

        public IStringLocalizer T { get; set; }
        public ILogger Logger { get; set; }

        public Task<IEnumerable<RecipeDescriptor>> HarvestRecipesAsync()
        {
            return _extensionManager.GetExtensions().InvokeAsync(descriptor => {
                return Task.FromResult(HarvestRecipes(descriptor));
            }, Logger);
        }

        public Task<IEnumerable<RecipeDescriptor>> HarvestRecipesAsync(string extensionId)
        {
            var descriptor = _extensionManager.GetExtension(extensionId);

            if (descriptor.Exists)
            {
                return Task.FromResult(HarvestRecipes(descriptor));
            }

            Logger.LogError(T["Could not discover recipes because extension '{0}' was not found.", extensionId]);
            return Task.FromResult(Enumerable.Empty<RecipeDescriptor>());
        }

        private IEnumerable<RecipeDescriptor> HarvestRecipes(IExtensionInfo extension)
        {
            var folderSubPath = Path.Combine(extension.SubPath, "Recipes");
            var recipeContainerFileInfo = _hostingEnvironment
                .ContentRootFileProvider
                .GetFileInfo(folderSubPath);

            var recipeOptions = _recipeOptions.Value;

            List<RecipeDescriptor> recipeDescriptors = new List<RecipeDescriptor>();

            var matcher = new Matcher(System.StringComparison.OrdinalIgnoreCase);
            matcher.AddInclude("*.recipe.json");

            var matches = matcher
                .Execute(new DirectoryInfoWrapper(new DirectoryInfo(recipeContainerFileInfo.PhysicalPath)))
                .Files;

            if (matches.Any())
            {
                var result = matches
                    .Select(match => _hostingEnvironment
                        .ContentRootFileProvider
                        .GetFileInfo(Path.Combine(folderSubPath, match.Path))).ToArray();

                recipeDescriptors.AddRange(result.Select(recipeFile =>
                {
                    // TODO: Try to optimize by only reading the required metadata instead of the whole file
                    using (StreamReader file = File.OpenText(recipeFile.PhysicalPath))
                    {
                        using (var reader = new JsonTextReader(file))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            var recipeDescriptor = serializer.Deserialize<RecipeDescriptor>(reader);
                            recipeDescriptor.RecipeFileInfo = recipeFile;
                            return recipeDescriptor;
                        }
                    }
                }));
            }

            return recipeDescriptors;
        }
    }
}
