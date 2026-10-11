using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Files;
using OrchardCore.Media;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Reads the rows of a CSV, JSON, JSON Lines or Excel file of the media library. The user the run is for must be
/// allowed to manage the media of the file's folder.
/// </summary>
public sealed class ReadMediaFileStep : DataPipelineStepType<ReadMediaFileStepSettings>
{
    /// <summary>
    /// The name of the step type.
    /// </summary>
    public const string StepName = "ReadMediaFile";

    private readonly IStringLocalizer S;

    public ReadMediaFileStep(IStringLocalizer<ReadMediaFileStep> localizer)
    {
        S = localizer;
    }

    public override string Name => StepName;

    public override LocalizedString DisplayName => S["Read a media file"];

    public override LocalizedString Description => S["Reads the rows of a CSV, JSON or Excel file of the media library."];

    public override DataPipelineStepCategory Category => DataPipelineStepCategory.Source;

    public override string Icon => "fa-solid fa-file-import";

    public override async Task DescribeAsync(DataPipelineDescribeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);
        var (path, format, error) = await ResolveAsync(settings, context.Services, context.User);

        if (error is not null)
        {
            context.AddError(error);
            context.SetOutputFields([]);

            return;
        }

        // Read the first batch to learn the columns.
        var store = context.Services.GetRequiredService<IMediaFileStore>();
        await using var stream = await OpenSeekableAsync(store, path, context.CancellationToken);
        var options = CreateOptions(settings);
        options.BatchSize = 50;

        await foreach (var batch in format.ReadAsync(stream, options, context.CancellationToken))
        {
            context.SetOutputFields(batch.Fields);

            return;
        }

        context.SetOutputFields([]);
    }

    public override async Task ExecuteAsync(DataPipelineStepContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);
        var (path, format, error) = await ResolveAsync(settings, context.Services, context.Run.User);

        if (error is not null)
        {
            throw new InvalidOperationException(error);
        }

        var store = context.Services.GetRequiredService<IMediaFileStore>();
        var output = context.GetOutput();
        var remaining = context.Run.IsPreview ? context.Run.PreviewRowLimit : long.MaxValue;

        await using var stream = await OpenSeekableAsync(store, path, context.CancellationToken);

        await foreach (var batch in format.ReadAsync(stream, CreateOptions(settings), context.CancellationToken))
        {
            if (output.IsClosed || remaining <= 0)
            {
                return;
            }

            var rows = batch.Rows.Count > remaining ? batch.Rows.Take((int)remaining).ToList() : batch.Rows;
            remaining -= rows.Count;

            await output.WriteAsync(new DataBatch(batch.Fields, rows), context.CancellationToken);
        }
    }

    private async Task<(string Path, IDataFileFormat Format, string Error)> ResolveAsync(ReadMediaFileStepSettings settings, IServiceProvider services, System.Security.Claims.ClaimsPrincipal user)
    {
        var path = MediaStepPaths.Normalize(settings.Path);

        if (string.IsNullOrEmpty(path))
        {
            return (null, null, S["Enter the path of a file of the media library."]);
        }

        var formats = services.GetRequiredService<IDataFileFormatManager>();
        var format = string.IsNullOrEmpty(settings.Format) ? formats.GetFormatByFileName(path) : formats.GetFormat(settings.Format);

        if (format is null)
        {
            return (path, null, S["The format of the file '{0}' is not supported. Choose its format.", path]);
        }

        var folder = Path.GetDirectoryName(path)?.Replace('\\', '/') ?? string.Empty;

        if (!await MediaStepPaths.CanManageAsync(services.GetRequiredService<IAuthorizationService>(), user, folder))
        {
            return (path, format, S["The user the pipeline runs for may not read the media of the folder '{0}'.", folder]);
        }

        if (await services.GetRequiredService<IMediaFileStore>().GetFileInfoAsync(path) is null)
        {
            return (path, format, S["The media library has no file '{0}'.", path]);
        }

        return (path, format, null);
    }

    private static DataFileOptions CreateOptions(ReadMediaFileStepSettings settings)
        => new()
        {
            Delimiter = string.IsNullOrEmpty(settings.Delimiter) ? "," : settings.Delimiter,
            HasHeaderRow = settings.HasHeaderRow,
            SheetName = settings.SheetName,
        };

    // Excel workbooks are zip packages, which need a stream that can seek; media stores may return one that can't.
    private static async Task<Stream> OpenSeekableAsync(IMediaFileStore store, string path, CancellationToken cancellationToken)
    {
        var stream = await store.GetFileStreamAsync(path);

        if (stream.CanSeek)
        {
            return stream;
        }

        var copy = new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.None, 16 * 1024, FileOptions.DeleteOnClose | FileOptions.Asynchronous);

        await using (stream)
        {
            await stream.CopyToAsync(copy, cancellationToken);
        }

        copy.Position = 0;

        return copy;
    }
}
