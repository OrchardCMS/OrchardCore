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
        private readonly string[] _recipies;
        private readonly Assembly[] _assemblies;

        public TestRecipeHarvester(IRecipeReader recipeReader, string[] recipies, Assembly[] assemblies)
        {
            _recipeReader = recipeReader;
            _recipies = recipies ?? new string[0];
            _assemblies = assemblies ?? new Assembly[0];
        }

        public Task<IEnumerable<RecipeDescriptor>> HarvestRecipesAsync()
            => HarvestRecipesAsync(_recipies);

        private async Task<IEnumerable<RecipeDescriptor>> HarvestRecipesAsync(string[] paths)
        {
            var recipeDescriptors = new List<RecipeDescriptor>();
            var fileInfos = new Dictionary<IFileInfo, EmbeddedFileProvider>();

            foreach (var assembly in _assemblies)
            {
                var testAssemblyFileProvider = new EmbeddedFileProvider(assembly);

                foreach (var path in paths)
                {
                    // EmbeddedFileProvider doesn't list directory contents.
                    var fileInfo = testAssemblyFileProvider.GetFileInfo(path);
                    if (fileInfo.Exists)
                    {
                        fileInfos.Add(fileInfo, testAssemblyFileProvider);
                    }
                }
            }

            foreach (var fileInfo in fileInfos)
            {
                var descriptor = await _recipeReader.GetRecipeDescriptor(fileInfo.Key.PhysicalPath, fileInfo.Key, fileInfo.Value);
                recipeDescriptors.Add(descriptor);
            }

            return recipeDescriptors;
        }
    }

    public class AssemblyRecipies
    {
        public AssemblyRecipies(Assembly assembly, string[] recipies)
        {
            Assembly = assembly;
            Recipies = recipies;
        }
        public Assembly Assembly { get; private set; }
        public string[] Recipies { get; private set; }

    }
}
