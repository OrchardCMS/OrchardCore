using Microsoft.Extensions.FileProviders;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Tests.Functional.Helpers;

/// <summary>
/// Serves recipes embedded in the test assembly, eliminating the need to copy
/// recipe files to the application's Recipes folder at runtime.
/// </summary>
public sealed class EmbeddedRecipeHarvester : IRecipeHarvester
{
    private readonly IRecipeReader _recipeReader;
    private readonly EmbeddedFileProvider _fileProvider;

    public EmbeddedRecipeHarvester(IRecipeReader recipeReader)
    {
        _recipeReader = recipeReader;
        _fileProvider = new EmbeddedFileProvider(typeof(EmbeddedRecipeHarvester).Assembly);
    }

    public async Task<IEnumerable<RecipeDescriptor>> HarvestRecipesAsync()
    {
        var recipes = new List<RecipeDescriptor>();
        var assembly = typeof(EmbeddedRecipeHarvester).Assembly;

        foreach (var resourceName in assembly.GetManifestResourceNames()
            .Where(n => n.EndsWith(".recipe.json", StringComparison.OrdinalIgnoreCase)))
        {
            var fileInfo = _fileProvider.GetFileInfo(resourceName);
            if (!fileInfo.Exists)
            {
                continue;
            }

            var descriptor = await _recipeReader.GetRecipeDescriptorAsync(
                string.Empty, fileInfo, _fileProvider);

            if (descriptor is not null)
            {
                recipes.Add(descriptor);
            }
        }

        return recipes;
    }
}
