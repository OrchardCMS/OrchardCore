using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Tests.Apis.Context;

public class TestRecipeHarvester : IRecipeHarvester
{
    private readonly IRecipeReader _recipeReader;

    public TestRecipeHarvester(IRecipeReader recipeReader)
    {
        _recipeReader = recipeReader;
    }

    public Task<IEnumerable<RecipeDescriptor>> HarvestRecipesAsync()
        => HarvestRecipesAsync(
        [
            "Apis/Lucene/Recipes/luceneQueryTest.json",
            "Apis/GraphQL/ContentManagement/Recipes/DynamicContentTypeQueryTest.json",
            "OrchardCore.Users/Recipes/UserSettingsTest.json"
        ]);

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

            if (descriptor == null)
            {
                continue;
            }

            recipeDescriptors.Add(descriptor);
        }

        return recipeDescriptors;
    }
}
