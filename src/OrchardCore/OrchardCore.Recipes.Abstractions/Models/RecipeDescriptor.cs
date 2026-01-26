using Json.Schema;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes.Models;

/// <summary>
/// Represents a recipe descriptor that provides metadata and content for a recipe.
/// This class implements <see cref="IRecipeDescriptor"/> for compatibility with the unified recipe system.
/// </summary>
public class RecipeDescriptor : IRecipeDescriptor
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public string WebSite { get; set; }
    public string Version { get; set; }
    public bool IsSetupRecipe { get; set; }
    public DateTime? ExportUtc { get; set; }
    public string[] Categories { get; set; }
    public string[] Tags { get; set; }
    public bool RequireNewScope { get; set; } = true;

    /// <summary>
    /// The path of the recipe file for the <see cref="RecipeDescriptor.FileProvider"/> property.
    /// </summary>
    public string BasePath { get; set; }

    public IFileInfo RecipeFileInfo { get; set; }
    public IFileProvider FileProvider { get; set; }


    public virtual Task<JsonSchema> GetSchemaAsync()
    {
        // File-based recipes don't have schema by default.
        // The schema is built from all registered steps via IRecipeSchemaService.
        return Task.FromResult<JsonSchema>(null);
    }

    /// <inheritdoc />
    public virtual Task<Stream> OpenReadStreamAsync()
    {
        if (RecipeFileInfo is null)
        {
            throw new InvalidOperationException("RecipeFileInfo is not set.");
        }

        return Task.FromResult(RecipeFileInfo.CreateReadStream());
    }
}
