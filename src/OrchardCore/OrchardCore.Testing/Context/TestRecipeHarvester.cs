using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Testing.Context
{
    public class TestRecipeHarvester : IRecipeHarvester
    {
        private readonly IRecipeReader _recipeReader;
        private readonly IEnumerable<RecipeLocator> _recipies;

        public TestRecipeHarvester(IRecipeReader recipeReader, IEnumerable<RecipeLocator> recipies)
        {
            _recipeReader = recipeReader;
            _recipies = recipies;
        }

        public Task<IEnumerable<RecipeDescriptor>> HarvestRecipesAsync() => HarvestAsync();

        private async Task<IEnumerable<RecipeDescriptor>> HarvestAsync()
        {
            var recipeDescriptors = new List<RecipeDescriptor>();

            if (_recipies != null)
            {
                foreach (var item in _recipies)
                {
                    var testAssemblyFileProvider = new EmbeddedFileProvider(item.Assembly);

                    // EmbeddedFileProvider doesn't list directory contents.
                    var fileInfo = testAssemblyFileProvider.GetFileInfo(item.Path);
                    if (fileInfo.Exists)
                    {
                        var descriptor = await _recipeReader.GetRecipeDescriptor(fileInfo.PhysicalPath, fileInfo, testAssemblyFileProvider);
                        recipeDescriptors.Add(descriptor);
                    }
                }
            }
            return recipeDescriptors;
        }
    }

    public class RecipeLocator
    {
        public RecipeLocator(string path, Assembly assembly)
        {
            Path = path;
            Assembly = assembly;
        }
        public Assembly Assembly { get; private set; }
        public string Path { get; private set; }

    }
}
