using System.Text.Json;
using Json.Schema;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// A recipe descriptor that loads recipe metadata from a file-based source.
/// Supports optional X.recipe.schema.json files for custom schema definitions.
/// </summary>
public sealed class FileRecipeDescriptor : IRecipeDescriptor
{
    private readonly IRecipeSchemaService _schemaService;
    private readonly ILogger _logger;
    private JsonSchema _schema;
    private bool _schemaLoaded;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileRecipeDescriptor"/> class.
    /// </summary>
    /// <param name="basePath">The base path of the recipe file.</param>
    /// <param name="recipeFileInfo">The file information for the recipe file.</param>
    /// <param name="fileProvider">The file provider to read the recipe and schema files.</param>
    /// <param name="schemaService">The recipe schema service for building schemas.</param>
    /// <param name="logger">The logger instance.</param>
    public FileRecipeDescriptor(
        string basePath,
        IFileInfo recipeFileInfo,
        IFileProvider fileProvider,
        IRecipeSchemaService schemaService,
        ILogger<FileRecipeDescriptor> logger)
    {
        ArgumentNullException.ThrowIfNull(recipeFileInfo);
        ArgumentNullException.ThrowIfNull(fileProvider);
        ArgumentNullException.ThrowIfNull(schemaService);

        BasePath = basePath;
        RecipeFileInfo = recipeFileInfo;
        FileProvider = fileProvider;
        _schemaService = schemaService;
        _logger = logger;
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

    /// <inheritdoc />
    public async Task<JsonSchema> GetSchemaAsync()
    {
        if (_schemaLoaded)
        {
            return _schema;
        }

        _schemaLoaded = true;

        // Try to load schema from X.recipe.schema.json file.
        var schemaFileName = GetSchemaFileName();
        var schemaFilePath = Path.Combine(BasePath, schemaFileName);
        var schemaFileInfo = FileProvider.GetFileInfo(schemaFilePath);

        if (schemaFileInfo.Exists)
        {
            try
            {
                await using var schemaStream = schemaFileInfo.CreateReadStream();
                _schema = await JsonSchema.FromStream(schemaStream);
                return _schema;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load schema from file: {SchemaFilePath}", schemaFilePath);
            }
        }

        // Fall back to building a schema from the combined step schemas.
        _schema = _schemaService.GetCombinedSchema();
        return _schema;
    }

    /// <inheritdoc />
    public Task<Stream> OpenReadStreamAsync()
    {
        return Task.FromResult(RecipeFileInfo.CreateReadStream());
    }

    /// <summary>
    /// Gets the schema file name based on the recipe file name.
    /// For example, "MyRecipe.recipe.json" would have a schema file named "MyRecipe.recipe.schema.json".
    /// </summary>
    private string GetSchemaFileName()
    {
        var fileName = RecipeFileInfo.Name;

        // Remove .recipe.json extension and add .recipe.schema.json.
        if (fileName.EndsWith(RecipesConstants.RecipeExtension, StringComparison.OrdinalIgnoreCase))
        {
            var baseName = fileName[..^RecipesConstants.RecipeExtension.Length];
            return $"{baseName}{RecipesConstants.RecipeSchemaExtension}";
        }

        // Fallback: just append .schema.json.
        return $"{fileName}.schema.json";
    }

    /// <summary>
    /// Creates a <see cref="FileRecipeDescriptor"/> from an existing <see cref="RecipeDescriptor"/>.
    /// This is used for backward compatibility with existing harvesters.
    /// </summary>
    /// <param name="descriptor">The existing recipe descriptor.</param>
    /// <param name="schemaService">The recipe schema service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <returns>A new <see cref="FileRecipeDescriptor"/> instance.</returns>
    public static FileRecipeDescriptor FromRecipeDescriptor(
        RecipeDescriptor descriptor,
        IRecipeSchemaService schemaService,
        ILogger<FileRecipeDescriptor> logger)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        return new FileRecipeDescriptor(
            descriptor.BasePath,
            descriptor.RecipeFileInfo,
            descriptor.FileProvider,
            schemaService,
            logger)
        {
            Name = descriptor.Name,
            DisplayName = descriptor.DisplayName,
            Description = descriptor.Description,
            Author = descriptor.Author,
            WebSite = descriptor.WebSite,
            Version = descriptor.Version,
            IsSetupRecipe = descriptor.IsSetupRecipe,
            ExportUtc = descriptor.ExportUtc,
            Categories = descriptor.Categories,
            Tags = descriptor.Tags,
            RequireNewScope = descriptor.RequireNewScope,
        };
    }
}
