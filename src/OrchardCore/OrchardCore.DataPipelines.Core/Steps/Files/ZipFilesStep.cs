using System.IO.Compression;
using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Models;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Compresses the files it reads into one zip archive. Files with the same name get a number appended.
/// </summary>
public sealed class ZipFilesStep : DataPipelineStepType<ZipFilesStepSettings>
{
    /// <summary>
    /// The name of the step type.
    /// </summary>
    public const string StepName = "ZipFiles";

    private readonly IStringLocalizer S;

    public ZipFilesStep(IStringLocalizer<ZipFilesStep> localizer)
    {
        S = localizer;
    }

    public override string Name => StepName;

    public override LocalizedString DisplayName => S["Zip files"];

    public override LocalizedString Description => S["Compresses files into one zip archive."];

    public override DataPipelineStepCategory Category => DataPipelineStepCategory.File;

    public override string Icon => "fa-solid fa-file-zipper";

    public override IReadOnlyList<DataPipelinePort> GetInputs(DataPipelineStep step)
        => [new(DataPipelinePort.Input, S["Files"], DataPipelinePortKind.Files) { IsRequired = true, AllowsMany = true }];

    public override Task DescribeAsync(DataPipelineDescribeContext context) => Task.CompletedTask;

    public override async Task ExecuteAsync(DataPipelineStepContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);
        var now = await DataPipelineFormulas.GetRunTimeAsync(context.Run);
        var name = DataPipelineTemplate.RenderFileName(string.IsNullOrWhiteSpace(settings.FileName) ? "{PipelineName}-{Date:yyyyMMdd-HHmmss}" : settings.FileName, context.Run, now);
        var archive = context.Run.CreateFile(DataPipelineTemplate.WithExtension(string.IsNullOrEmpty(name) ? "files" : name, ".zip"), "application/zip");
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var count = 0;

        await using (var stream = new FileStream(archive.Path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 16 * 1024, FileOptions.Asynchronous))
        await using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: false))
        {
            await foreach (var file in context.GetInput().ReadFilesAsync(context.CancellationToken))
            {
                var entryName = Unique(file.FileName, names);
                var entry = zip.CreateEntry(entryName, CompressionLevel.Optimal);

                await using var entryStream = await entry.OpenAsync(context.CancellationToken);
                await using var source = file.OpenRead();
                await source.CopyToAsync(entryStream, context.CancellationToken);
                count++;
            }
        }

        context.LogInformation(S["Zipped {0} files into '{1}'.", count, archive.FileName]);

        await context.GetOutput().WriteFileAsync(archive, context.CancellationToken);
    }

    private static string Unique(string fileName, HashSet<string> names)
    {
        var candidate = fileName;
        var number = 2;

        while (!names.Add(candidate))
        {
            candidate = $"{Path.GetFileNameWithoutExtension(fileName)} ({number++}){Path.GetExtension(fileName)}";
        }

        return candidate;
    }
}
