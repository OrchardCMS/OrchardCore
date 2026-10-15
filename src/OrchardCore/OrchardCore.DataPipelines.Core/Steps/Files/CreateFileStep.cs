using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataSources.Files;
using OrchardCore.DataSources;
using OrchardCore.Localization;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Writes the rows it reads to a file, such as a CSV file or an Excel workbook, which the next steps deliver. The rows
/// are written as they arrive, so a large file never needs to fit in memory. With several connections, a workbook
/// gets a worksheet per connection, named after the step it comes from; other formats write the rows of every
/// connection, mapped by name to the fields of the first one.
/// </summary>
public sealed class CreateFileStep : DataPipelineStepType<CreateFileStep, CreateFileStepSettings>, IDataPipelineStepType
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

    public static string Name => StepName;

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<CreateFileStep>("Create a file");

    public static LocalizationSource Description { get; } = LocalizationSource.Create<CreateFileStep>("Writes the rows to a CSV, JSON or Excel file.");

    public static DataPipelineStepCategory Category => DataPipelineStepCategory.File;

    public static string Icon => "fa-solid fa-file-export";

    public override IReadOnlyList<DataPipelinePort> GetInputs(DataPipelineStep step)
    {
        return [new(DataPipelinePort.Input, S["Rows"]) { IsRequired = true, AllowsMany = true }];
    }

    public override Task DescribeAsync(DataPipelineDescribeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);

        if (context.Services?.GetService<IDataFileFormatManager>()?.GetDescriptor(settings.Format) is null)
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
        var formats = context.Services?.GetService<IDataFileFormatManager>();
        var descriptor = formats?.GetDescriptor(settings.Format)
            ?? throw new InvalidOperationException(S["The file format '{0}' is not available.", settings.Format ?? string.Empty]);
        var format = formats.GetFormat(descriptor.Name);

        var now = await DataPipelineFormulas.GetRunTimeAsync(context.Run);
        var name = DataPipelineTemplate.RenderFileName(string.IsNullOrWhiteSpace(settings.FileName) ? "{PipelineName}-{Date:yyyyMMdd-HHmmss}" : settings.FileName, context.Run, now);

        if (string.IsNullOrEmpty(name))
        {
            name = "export";
        }

        var file = context.Run.CreateFile(DataPipelineTemplate.WithExtension(name, descriptor.Extension), descriptor.ContentType);
        var input = context.GetInput();

        var options = new DataFileOptions
        {
            Delimiter = string.IsNullOrEmpty(settings.Delimiter) ? "," : settings.Delimiter,
            HasHeaderRow = settings.IncludeHeader,
            UseDisplayNames = settings.UseDisplayNames,
            Indented = settings.Indented,
        };

        var sheetNames = (settings.SheetName ?? string.Empty).Split('\n').Select(line => line.Trim()).ToArray();
        long count;

        await using (var stream = new FileStream(file.Path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 16 * 1024, FileOptions.Asynchronous))
        {
            if (format is IDataWorkbookFormat workbook && input.FieldSets.Count > 1)
            {
                count = await WriteSheetsAsync(workbook, stream, input, sheetNames, options, context.CancellationToken);
            }
            else
            {
                options.SheetName = sheetNames.FirstOrDefault(sheetName => sheetName.Length > 0);

                // The fields of the first batch win over the described ones, in case they differ at run time.
                await using var reader = await DataPipelineRowReader.OpenAsync(input, context.CancellationToken);

                count = await format.WriteAsync(stream, reader.Fields, ToBatches(reader.Fields, reader.ReadRowsAsync()), options, context.CancellationToken);
            }
        }

        file.RowCount = count;

        context.LogInformation(S["Created the file '{0}' with {1} rows.", file.FileName, count]);

        await context.GetOutput().WriteFileAsync(file, context.CancellationToken);
    }

    // Writes each connection to its own worksheet, named with the line of the sheet names at its position, or after
    // the step it comes from.
    private static async Task<long> WriteSheetsAsync(
        IDataWorkbookFormat workbook,
        Stream stream,
        DataPipelineInput input,
        string[] sheetNames,
        DataFileOptions options,
        CancellationToken cancellationToken)
    {
        var sheets = new List<DataSheet>();
        var enumerators = new List<IAsyncEnumerator<DataBatch>>();

        try
        {
            for (var connection = 0; connection < input.FieldSets.Count; connection++)
            {
                // The fields of the first batch win over the described ones, in case they differ at run time.
                var enumerator = input.ReadConnectionBatchesAsync(connection, cancellationToken).GetAsyncEnumerator(cancellationToken);
                enumerators.Add(enumerator);

                var first = await enumerator.MoveNextAsync() ? enumerator.Current : null;
                var name = connection < sheetNames.Length && sheetNames[connection].Length > 0
                    ? sheetNames[connection]
                    : connection < input.ConnectionLabels.Count ? input.ConnectionLabels[connection] : null;

                sheets.Add(new DataSheet(name, first?.Fields ?? input.FieldSets[connection], Remaining(first, enumerator, cancellationToken)));
            }

            var counts = await workbook.WriteSheetsAsync(stream, sheets, options, cancellationToken);

            return counts.Sum();
        }
        finally
        {
            foreach (var enumerator in enumerators)
            {
                await enumerator.DisposeAsync();
            }
        }
    }

    private static async IAsyncEnumerable<DataBatch> ToBatches(IReadOnlyList<DataField> fields, IAsyncEnumerable<object[]> rows)
    {
        var batch = new List<object[]>(DataSourceQuery.DefaultBatchSize);

        await foreach (var row in rows)
        {
            batch.Add(row);

            if (batch.Count == DataSourceQuery.DefaultBatchSize)
            {
                yield return new DataBatch(fields, batch);
                batch = new List<object[]>(DataSourceQuery.DefaultBatchSize);
            }
        }

        if (batch.Count > 0)
        {
            yield return new DataBatch(fields, batch);
        }
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
}
