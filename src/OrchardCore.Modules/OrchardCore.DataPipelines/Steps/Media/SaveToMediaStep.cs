using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.FileStorage;
using OrchardCore.Media;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Saves the files it reads to a folder of the media library. It checks that the user the run is for may manage the
/// media of the folder, and that the media library accepts the extension of each file.
/// </summary>
public sealed class SaveToMediaStep : DataPipelineStepType<SaveToMediaStepSettings>
{
    /// <summary>
    /// The name of the step type.
    /// </summary>
    public const string StepName = "SaveToMedia";

    private readonly IStringLocalizer S;

    public SaveToMediaStep(IStringLocalizer<SaveToMediaStep> localizer)
    {
        S = localizer;
    }

    public override string Name => StepName;

    public override LocalizedString DisplayName => S["Save to the media library"];

    public override LocalizedString Description => S["Saves files to a folder of the media library."];

    public override DataPipelineStepCategory Category => DataPipelineStepCategory.Destination;

    public override string Icon => "fa-solid fa-photo-film";

    public override Task DescribeAsync(DataPipelineDescribeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (MediaStepPaths.Normalize(GetSettings(context.Step).Folder) is null)
        {
            context.AddError(S["The folder must be inside the media library."]);
        }

        return Task.CompletedTask;
    }

    public override async Task ExecuteAsync(DataPipelineStepContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);
        var folder = MediaStepPaths.Normalize(settings.Folder)
            ?? throw new InvalidOperationException(S["The folder must be inside the media library."]);

        var store = context.Services.GetRequiredService<IMediaFileStore>();
        var options = context.Services.GetRequiredService<IOptions<MediaOptions>>().Value;
        var authorizationService = context.Services.GetRequiredService<IAuthorizationService>();

        if (!await MediaStepPaths.CanManageAsync(authorizationService, context.Run.User, folder))
        {
            throw new InvalidOperationException(S["The user the pipeline runs for may not manage the media of the folder '{0}'.", folder]);
        }

        await foreach (var file in context.GetInput().ReadFilesAsync(context.CancellationToken))
        {
            var extension = Path.GetExtension(file.FileName);

            if (options.AllowedFileExtensions is { Count: > 0 } allowed && !allowed.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(S["The media library doesn't accept files with the extension '{0}'.", extension]);
            }

            if (options.MaxFileSize > 0 && file.Length > options.MaxFileSize)
            {
                throw new InvalidOperationException(S["The file '{0}' is larger than the media library accepts.", file.FileName]);
            }

            var path = await GetPathAsync(store, folder, file.FileName, settings.Overwrite);

            await using (var stream = file.OpenRead())
            {
                await store.CreateFileFromStreamAsync(path, stream, overwrite: settings.Overwrite);
            }

            context.AddDelivery(S["Saved '{0}' to the media library.", path], store.MapPathToPublicUrl(path));
        }
    }

    private static async Task<string> GetPathAsync(IMediaFileStore store, string folder, string fileName, bool overwrite)
    {
        var path = string.IsNullOrEmpty(folder) ? fileName : store.Combine(folder, fileName);

        if (overwrite)
        {
            return path;
        }

        var name = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        var number = 2;

        while (await store.GetFileInfoAsync(path) is not null)
        {
            var candidate = $"{name} ({number++}){extension}";
            path = string.IsNullOrEmpty(folder) ? candidate : store.Combine(folder, candidate);
        }

        return path;
    }
}
