using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// A schema provider that loads JSON schema from a file.
/// This allows modules to define their recipe step schemas in JSON files.
/// </summary>
public class FileBasedRecipeStepSchemaProvider : IRecipeStepSchemaProvider
{
    private readonly IFileProvider _fileProvider;
    private readonly string _schemaFilePath;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private volatile JsonObject _cachedSchema;

    /// <summary>
    /// Creates a new instance of <see cref="FileBasedRecipeStepSchemaProvider"/>.
    /// </summary>
    /// <param name="stepName">The name of the recipe step this schema is for.</param>
    /// <param name="fileProvider">The file provider to use for loading the schema file.</param>
    /// <param name="schemaFilePath">The path to the JSON schema file relative to the file provider.</param>
    /// <param name="logger">The logger instance.</param>
    public FileBasedRecipeStepSchemaProvider(
        string stepName,
        IFileProvider fileProvider,
        string schemaFilePath,
        ILogger<FileBasedRecipeStepSchemaProvider> logger)
    {
        ArgumentException.ThrowIfNullOrEmpty(stepName);
        ArgumentNullException.ThrowIfNull(fileProvider);
        ArgumentException.ThrowIfNullOrEmpty(schemaFilePath);

        StepName = stepName;
        _fileProvider = fileProvider;
        _schemaFilePath = schemaFilePath;
        _logger = logger;
    }

    /// <inheritdoc />
    public string StepName { get; }

    /// <inheritdoc />
    public async ValueTask<JsonObject> GetSchemaAsync()
    {
        // Fast path: check if already cached.
        if (_cachedSchema is not null)
        {
            return _cachedSchema;
        }

        await _semaphore.WaitAsync();
        try
        {
            // Double-check after acquiring lock.
            if (_cachedSchema is not null)
            {
                return _cachedSchema;
            }

            var fileInfo = _fileProvider.GetFileInfo(_schemaFilePath);

            if (!fileInfo.Exists)
            {
                _logger.LogWarning("Schema file not found for recipe step '{StepName}' at path '{SchemaFilePath}'.",
                    StepName, _schemaFilePath);
                return null;
            }

            await using var stream = fileInfo.CreateReadStream();
            var jsonDocument = await JsonDocument.ParseAsync(stream);
            _cachedSchema = JsonObject.Create(jsonDocument.RootElement);
            return _cachedSchema;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse JSON schema file for recipe step '{StepName}' at path '{SchemaFilePath}'.",
                StepName, _schemaFilePath);
            return null;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
