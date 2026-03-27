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
        var assemblyPrefix = assembly.GetName().Name + ".";

        foreach (var resourceName in assembly.GetManifestResourceNames()
            .Where(n => n.EndsWith(".recipe.json", StringComparison.OrdinalIgnoreCase)))
        {
            string subpath;
            if (resourceName.StartsWith(assemblyPrefix, StringComparison.Ordinal))
            {
                // Trim the assembly name prefix to get a name relative to the provider base namespace.
                var trimmed = resourceName.Substring(assemblyPrefix.Length);

                // Convert the first namespace separator to a path separator (e.g. "Recipes.Foo.recipe.json" -> "Recipes/Foo.recipe.json"),
                // which matches the subpath format expected by EmbeddedFileProvider.
                var firstDotIndex = trimmed.IndexOf('.');
                if (firstDotIndex >= 0)
                {
                    subpath = trimmed.Substring(0, firstDotIndex) + "/" + trimmed.Substring(firstDotIndex + 1);
                }
                else
                {
                    subpath = trimmed;
                }
            }
            else
            {
                // Fallback: use the resource name as-is if it does not start with the assembly prefix.
                subpath = resourceName;
            }

            var fileInfo = _fileProvider.GetFileInfo(subpath);
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
