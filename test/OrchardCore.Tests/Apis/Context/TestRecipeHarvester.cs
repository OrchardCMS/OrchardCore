using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Tests.Apis.Context
{
    public class TestRecipeHarvester : IRecipeHarvester
    {
        private readonly IRecipeReader _recipeReader;

        public TestRecipeHarvester(IRecipeReader recipeReader)
        {
            _recipeReader = recipeReader;
        }

        public Task<IEnumerable<RecipeDescriptor>> HarvestRecipesAsync()
            => HarvestRecipesAsync(new[]
            {
                "Apis/Lucene/Recipes/luceneQueryTest.json"
            });

        private async Task<IEnumerable<RecipeDescriptor>> HarvestRecipesAsync(string[] paths)
        {
            var recipeDescriptors = new List<RecipeDescriptor>();
            var testAssemblyFileProvider = new EmbeddedFileProvider(GetType().GetTypeInfo().Assembly);
            var fileInfos = new List<IFileInfo>();

            foreach (var path in paths)
            {
                // EmbeddedFileProvider doesn't list directory contents.
                var fileInfo = testAssemblyFileProvider.GetFileInfo(path);
                Assert.True(fileInfo.Exists);
                fileInfos.Add(fileInfo);
            }

            foreach (var fileInfo in fileInfos)
            {
                var descriptor = await _recipeReader.GetRecipeDescriptorAsync(fileInfo.PhysicalPath, fileInfo, testAssemblyFileProvider);
                recipeDescriptors.Add(descriptor);
            }

            return recipeDescriptors;
        }
    }
}
