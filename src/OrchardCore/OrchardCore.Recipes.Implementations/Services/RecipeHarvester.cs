using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrchardCore.Environment.Extensions;
using OrchardCore.Modules;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services
{
    public class RecipeHarvester : IRecipeHarvester
    {
        private readonly IExtensionManager _extensionManager;
        private readonly IHostingEnvironment _hostingEnvironment;

        public RecipeHarvester(
            IExtensionManager extensionManager,
            IHostingEnvironment hostingEnvironment,
            ILogger<RecipeHarvester> logger)
        {
            _extensionManager = extensionManager;
            _hostingEnvironment = hostingEnvironment;

            Logger = logger;
        }

        public ILogger Logger { get; set; }

        public Task<IEnumerable<RecipeDescriptor>> HarvestRecipesAsync()
        {
            return _extensionManager.GetExtensions().InvokeAsync(descriptor => HarvestRecipes(descriptor), Logger);
        }
        
        private Task<IEnumerable<RecipeDescriptor>> HarvestRecipes(IExtensionInfo extension)
        {
            var folderSubPath = Path.Combine(extension.SubPath, "Recipes");
            return HarvestRecipesAsync(folderSubPath, _hostingEnvironment);
        }

        /// <summary>
        /// Returns a list of recipes for a content path.
        /// </summary>
        /// <param name="path">A path string relative to the content root of the application.</param>
        /// <returns>The list of <see cref="RecipeDescriptor"/> instances.</returns>
        public static Task<IEnumerable<RecipeDescriptor>> HarvestRecipesAsync(string path, IHostingEnvironment hostingEnvironment)
        {
            var recipeContainerFileInfo = hostingEnvironment
                .ContentRootFileProvider
                .GetFileInfo(path);

            var recipeDescriptors = new List<RecipeDescriptor>();

            var recipeFiles = hostingEnvironment.ContentRootFileProvider.GetDirectoryContents(path)
                .Where(x => !x.IsDirectory && x.Name.EndsWith(".recipe.json"));

            if (recipeFiles.Any())
            {
                recipeDescriptors.AddRange(recipeFiles.Select(recipeFile =>
                {
                    // TODO: Try to optimize by only reading the required metadata instead of the whole file

                    using (var stream = recipeFile.CreateReadStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            using (var jsonReader = new JsonTextReader(reader))
                            {
                                var serializer = new JsonSerializer();
                                var recipeDescriptor = serializer.Deserialize<RecipeDescriptor>(jsonReader);
                                recipeDescriptor.FileProvider = hostingEnvironment.ContentRootFileProvider;
                                recipeDescriptor.BasePath = path;
                                recipeDescriptor.RecipeFileInfo = recipeFile;

                                return recipeDescriptor;
                            }
                        }
                    }
                }));
            }

            return Task.FromResult<IEnumerable<RecipeDescriptor>>(recipeDescriptors);
        }
    }
}