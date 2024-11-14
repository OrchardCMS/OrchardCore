using System.Text.Json.Nodes;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Media.Recipes;

/// <summary>
/// This recipe step creates a set of queries.
/// </summary>
public sealed class MediaStep : NamedRecipeStepHandler
{
    private readonly IMediaFileStore _mediaFileStore;
    private readonly HashSet<string> _allowedFileExtensions;
    private readonly IHttpClientFactory _httpClientFactory;

    internal readonly IStringLocalizer S;

    public MediaStep(
        IMediaFileStore mediaFileStore,
        IOptions<MediaOptions> options,
        IHttpClientFactory httpClientFactory,
        IStringLocalizer<MediaStep> stringLocalizer)
        : base("media")
    {
        _mediaFileStore = mediaFileStore;
        _allowedFileExtensions = options.Value.AllowedFileExtensions;
        _httpClientFactory = httpClientFactory;
        S = stringLocalizer;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<MediaStepModel>();

        foreach (var file in model.Files)
        {
            if (!_allowedFileExtensions.Contains(Path.GetExtension(file.TargetPath), StringComparer.OrdinalIgnoreCase))
            {
                context.Errors.Add(S["File extension not allowed: '{0}'", file.TargetPath]);

                continue;
            }

            Stream stream = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(file.Base64))
                {
                    stream = Base64.DecodedToStream(file.Base64);
                }
                else if (!string.IsNullOrWhiteSpace(file.SourcePath))
                {
                    var fileInfo = context.RecipeDescriptor.FileProvider.GetRelativeFileInfo(context.RecipeDescriptor.BasePath, file.SourcePath);

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
                    await _mediaFileStore.CreateFileFromStreamAsync(file.TargetPath, stream, true);
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

    private sealed class MediaStepModel
    {
        /// <summary>
        /// Collection of <see cref="MediaStepFile"/> objects.
        /// </summary>
        public MediaStepFile[] Files { get; set; }
    }

    private sealed class MediaStepFile
    {
        /// <summary>
        /// Path where the content will be written.
        /// Use inter-changeably with <see cref="TargetPath"/>.
        /// </summary>
        public string Path { get => TargetPath; set => TargetPath = value; }

        /// <summary>
        /// Path where the content will be written.
        /// Use inter-changeably with <see cref="Path"/>.
        /// </summary>
        public string TargetPath { get; set; }

        /// <summary>
        /// Base64 encoded content. Use when the source file will
        /// not be available in this recipe step's file provider.
        /// If both this and SourcePath properties are set with
        /// non-null values, this property will be used.
        /// </summary>
        public string Base64 { get; set; }

        /// <summary>
        /// Path where the content is read from. Use when the file
        /// will be available in this recipe step's file provider.
        /// If both this and Base64 properties are set with
        /// non-null values, the Base64 property will be used.
        /// </summary>
        public string SourcePath { get; set; }

        /// <summary>
        /// URL where the content is read from. Use when the file
        /// will be available on a remote website.
        /// If Base64 property or SourcePath property are set, they take
        /// precedence over SourceUrl.
        /// </summary>
        public string SourceUrl { get; set; }
    }
}
