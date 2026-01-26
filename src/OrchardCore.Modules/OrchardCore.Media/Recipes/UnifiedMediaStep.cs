using Json.Schema;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Media.Recipes;

/// <summary>
/// Unified recipe step for importing media files.
/// </summary>
public sealed class UnifiedMediaStep : RecipeImportStep<UnifiedMediaStep.MediaStepModel>
{
    private readonly IMediaFileStore _mediaFileStore;
    private readonly HashSet<string> _allowedFileExtensions;
    private readonly IHttpClientFactory _httpClientFactory;

    internal readonly IStringLocalizer S;

    public UnifiedMediaStep(
        IMediaFileStore mediaFileStore,
        IOptions<MediaOptions> options,
        IHttpClientFactory httpClientFactory,
        IStringLocalizer<UnifiedMediaStep> stringLocalizer)
    {
        _mediaFileStore = mediaFileStore;
        _allowedFileExtensions = options.Value.AllowedFileExtensions;
        _httpClientFactory = httpClientFactory;
        S = stringLocalizer;
    }

    /// <inheritdoc />
    public override string Name => "media";

    /// <inheritdoc />
    protected override JsonSchema BuildSchema()
    {
        return new JsonSchemaBuilder()
            .Schema(MetaSchemas.Draft202012Id)
            .Type(SchemaValueType.Object)
            .Title(Name)
            .Description("Imports media files into the media library.")
            .Required("name", "Files")
            .Properties(
                ("name", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("Files", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Array)
                    .Description("Array of media files to import.")
                    .Items(new JsonSchemaBuilder()
                        .Type(SchemaValueType.Object)
                        .Required("TargetPath")
                        .Properties(
                            ("TargetPath", new JsonSchemaBuilder()
                                .Type(SchemaValueType.String)
                                .Description("Path where the file will be stored in the media library.")),
                            ("Path", new JsonSchemaBuilder()
                                .Type(SchemaValueType.String)
                                .Description("Alias for TargetPath.")),
                            ("Base64", new JsonSchemaBuilder()
                                .Type(SchemaValueType.String)
                                .Description("Base64 encoded file content.")),
                            ("SourcePath", new JsonSchemaBuilder()
                                .Type(SchemaValueType.String)
                                .Description("Relative path to source file (file-based recipes only).")),
                            ("SourceUrl", new JsonSchemaBuilder()
                                .Type(SchemaValueType.String)
                                .Format(Formats.Uri)
                                .Description("URL to download the file from."))))))
            .AdditionalProperties(false)
            .Build();
    }

    /// <inheritdoc />
    protected override async Task ImportAsync(MediaStepModel model, RecipeExecutionContext context)
    {
        foreach (var file in model.Files ?? [])
        {
            var targetPath = file.TargetPath ?? file.Path;
            if (string.IsNullOrWhiteSpace(targetPath))
            {
                context.Errors.Add(S["TargetPath is required for media files."]);
                continue;
            }

            if (!_allowedFileExtensions.Contains(System.IO.Path.GetExtension(targetPath), StringComparer.OrdinalIgnoreCase))
            {
                context.Errors.Add(S["File extension not allowed: '{0}'", targetPath]);
                continue;
            }

            Stream stream = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(file.Base64))
                {
                    stream = Base64.DecodeToStream(file.Base64);
                }
                else if (!string.IsNullOrWhiteSpace(file.SourcePath))
                {
                    // SourcePath requires a file-based recipe with FileProvider context.
                    if (context.RecipeDescriptor is not RecipeDescriptor legacyDescriptor || legacyDescriptor.FileProvider is null)
                    {
                        context.Errors.Add(S["SourcePath is only supported for file-based recipes. Use Base64 or SourceUrl instead for code-based recipes."]);
                        continue;
                    }

                    var fileInfo = legacyDescriptor.FileProvider.GetRelativeFileInfo(legacyDescriptor.BasePath, file.SourcePath);
                    stream = fileInfo.CreateReadStream();
                }
                else if (!string.IsNullOrWhiteSpace(file.SourceUrl))
                {
                    var httpClient = _httpClientFactory.CreateClient();
                    var response = await httpClient.GetAsync(file.SourceUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        stream = await response.Content.ReadAsStreamAsync();
                    }
                }

                if (stream != null)
                {
                    await _mediaFileStore.CreateFileFromStreamAsync(targetPath, stream, true);
                }
            }
            finally
            {
                if (stream != null)
                {
                    await stream.DisposeAsync();
                }
            }
        }
    }

    /// <summary>
    /// Model for the Media step data.
    /// </summary>
    public sealed class MediaStepModel
    {
        public MediaFile[] Files { get; set; }
    }

    /// <summary>
    /// Media file definition.
    /// </summary>
    public sealed class MediaFile
    {
        public string Path { get; set; }
        public string TargetPath { get; set; }
        public string Base64 { get; set; }
        public string SourcePath { get; set; }
        public string SourceUrl { get; set; }
    }
}
