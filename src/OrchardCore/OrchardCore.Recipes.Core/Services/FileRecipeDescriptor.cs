using Microsoft.Extensions.FileProviders;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// A recipe descriptor that loads recipe metadata from a file-based source.
/// Supports optional X.recipe.schema.json files for custom schema definitions.
/// </summary>
public sealed class FileRecipeDescriptor : IRecipeDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileRecipeDescriptor"/> class.
    /// </summary>
    /// <param name="basePath">The base path of the recipe file.</param>
    /// <param name="recipeFileInfo">The file information for the recipe file.</param>
    /// <param name="fileProvider">The file provider to read the recipe and schema files.</param>
    public FileRecipeDescriptor(
        string basePath,
        IFileInfo recipeFileInfo,
        IFileProvider fileProvider)
    {
        ArgumentNullException.ThrowIfNull(recipeFileInfo);
        ArgumentNullException.ThrowIfNull(fileProvider);

        BasePath = basePath;
        RecipeFileInfo = recipeFileInfo;
        FileProvider = fileProvider;
    }

    /// <inheritdoc />
    public string Name { get; set; }

    /// <inheritdoc />
    public string DisplayName { get; set; }

    /// <inheritdoc />
    public string Description { get; set; }

    /// <inheritdoc />
    public string Author { get; set; }

    /// <inheritdoc />
    public string WebSite { get; set; }

    /// <inheritdoc />
    public string Version { get; set; }

    /// <inheritdoc />
    public bool IsSetupRecipe { get; set; }

    /// <inheritdoc />
    public DateTime? ExportUtc { get; set; }

    /// <inheritdoc />
    public string[] Categories { get; set; }

    /// <inheritdoc />
    public string[] Tags { get; set; }

    /// <inheritdoc />
    public bool RequireNewScope { get; set; } = true;

    /// <summary>
    /// Gets the base path of the recipe file.
    /// </summary>
    public string BasePath { get; }

    /// <summary>
    /// Gets the file information for the recipe file.
    /// </summary>
    public IFileInfo RecipeFileInfo { get; }

    /// <summary>
    /// Gets the file provider used to read the recipe and schema files.
    /// </summary>
    public IFileProvider FileProvider { get; }

    public bool IsAvailable(ShellSettings shellSettings)
        => Tags is null || !Tags.Contains("hidden", StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public Task<Stream> OpenReadStreamAsync()
    {
        return Task.FromResult(RecipeFileInfo.CreateReadStream());
    }
}
