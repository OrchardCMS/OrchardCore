using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Files;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Writes the rows it reads to a file, such as a CSV file or an Excel workbook, which the next steps deliver. The rows
/// are written as they arrive, so a large file never needs to fit in memory.
/// </summary>
public sealed class CreateFileStep : DataPipelineStepType<CreateFileStepSettings>
{
    /// <summary>
    /// The name of the step type.
    /// </summary>
    public const string StepName = "CreateFile";

    private readonly IStringLocalizer S;

    public CreateFileStep(IStringLocalizer<CreateFileStep> localizer)
    {
        S = localizer;
    }

    public override string Name => StepName;

    public override LocalizedString DisplayName => S["Create a file"];

    public override LocalizedString Description => S["Writes the rows to a CSV, JSON or Excel file."];

    public override DataPipelineStepCategory Category => DataPipelineStepCategory.File;

    public override string Icon => "fa-solid fa-file-export";

    public override Task DescribeAsync(DataPipelineDescribeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);

        if (GetFormat(context.Services, settings) is null)
        {
            context.AddError(S["Choose the format of the file."]);
        }

        if (context.GetInputFields().Count == 0)
        {
            context.AddWarning(S["The rows of the input have no fields, so the file will be empty."]);
        }

        return Task.CompletedTask;
    }

    public override async Task ExecuteAsync(DataPipelineStepContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);
        var format = GetFormat(context.Services, settings)
            ?? throw new InvalidOperationException(S["The file format '{0}' is not available.", settings.Format ?? string.Empty]);

        var now = await DataPipelineFormulas.GetRunTimeAsync(context.Run);
        var name = DataPipelineTemplate.RenderFileName(string.IsNullOrWhiteSpace(settings.FileName) ? "{PipelineName}-{Date:yyyyMMdd-HHmmss}" : settings.FileName, context.Run, now);

        if (string.IsNullOrEmpty(name))
        {
            name = "export";
        }

        var file = context.Run.CreateFile(DataPipelineTemplate.WithExtension(name, format.Extension), format.ContentType);
        var input = context.GetInput();

        // The fields of the first batch win over the described ones, in case they differ at run time.
        await using var enumerator = input.ReadBatchesAsync(context.CancellationToken).GetAsyncEnumerator(context.CancellationToken);
        var first = await enumerator.MoveNextAsync() ? enumerator.Current : null;
        var fields = first?.Fields ?? input.Fields;

        var options = new DataFileOptions
        {
            Delimiter = string.IsNullOrEmpty(settings.Delimiter) ? "," : settings.Delimiter,
            HasHeaderRow = settings.IncludeHeader,
            UseDisplayNames = settings.UseDisplayNames,
            SheetName = settings.SheetName,
            Indented = settings.Indented,
        };

        long count;

        await using (var stream = new FileStream(file.Path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 16 * 1024, FileOptions.Asynchronous))
        {
            count = await format.WriteAsync(stream, fields, Remaining(first, enumerator, context.CancellationToken), options, context.CancellationToken);
        }

        file.RowCount = count;

        context.LogInformation(S["Created the file '{0}' with {1} rows.", file.FileName, count]);

        await context.GetOutput().WriteFileAsync(file, context.CancellationToken);
    }

    private static async IAsyncEnumerable<DataBatch> Remaining(DataBatch first, IAsyncEnumerator<DataBatch> enumerator, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (first is null)
        {
            yield break;
        }

        yield return first;

        while (await enumerator.MoveNextAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return enumerator.Current;
        }
    }

    private static IDataFileFormat GetFormat(IServiceProvider services, CreateFileStepSettings settings)
        => services?.GetService<IDataFileFormatManager>()?.GetFormat(settings.Format);
}
