using System.Text.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

public class RecipeReader : IRecipeReader
{
    private readonly ILogger _logger;

    public RecipeReader(ILogger<RecipeReader> logger)
    {
        _logger = logger;
    }

    public async Task<RecipeDescriptor> GetRecipeDescriptorAsync(string recipeBasePath, IFileInfo recipeFileInfo, IFileProvider recipeFileProvider)
    {
        // TODO: Try to optimize by only reading the required metadata instead of the whole file.
        using var stream = recipeFileInfo.CreateReadStream();

        RecipeDescriptor recipeDescriptor = null;

        try
        {
            recipeDescriptor = await JsonSerializer.DeserializeAsync<RecipeDescriptor>(stream, JOptions.Default);

            recipeDescriptor.FileProvider = recipeFileProvider;
            recipeDescriptor.BasePath = recipeBasePath;
            recipeDescriptor.RecipeFileInfo = recipeFileInfo;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to deserialize the recipe file: '{FileName}'.", recipeFileInfo.Name);
        }

        return recipeDescriptor;
    }
}
