using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.DataOrchestrator.Exporting;
using OrchardCore.DataOrchestrator.Models;

namespace OrchardCore.DataOrchestrator.Activities;

/// <summary>
/// Base class for load activities that serialize the pipeline data to a file using a
/// configurable <see cref="IEtlExportFormat"/> and write it to a destination.
/// </summary>
/// <remarks>
/// Concrete destinations (for example the media library or an FTP(S) server) only need to
/// implement <see cref="WriteToDestinationAsync"/>. Serialization is delegated to the
/// registered export formats so that every destination supports the same set of formats and
/// new formats become available to all destinations automatically.
/// </remarks>
public abstract class EtlFileExportLoad : EtlLoadActivity
{
    /// <summary>
    /// The name of the format selected by default when none is configured.
    /// </summary>
    public const string DefaultFormatName = "Json";

    /// <summary>
    /// Gets or sets the technical name of the <see cref="IEtlExportFormat"/> used to serialize the data.
    /// </summary>
    public string Format
    {
        get => GetProperty(() => DefaultFormatName);
        set => SetProperty(value);
    }

    /// <summary>
    /// Gets or sets the name (or relative path) of the file written to the destination.
    /// When empty, a default name based on the selected format is used.
    /// </summary>
    public string FileName
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public override IEnumerable<EtlOutcome> GetPossibleOutcomes()
    {
        return [new EtlOutcome("Done"), new EtlOutcome("Failed")];
    }

    public override async Task<EtlActivityResult> ExecuteAsync(EtlExecutionContext context)
    {
        var data = context.DataStream;

        if (data == null)
        {
            return EtlActivityResult.Failure("No data stream available.");
        }

        var formatProvider = context.ServiceProvider.GetService<IEtlExportFormatProvider>();

        if (formatProvider == null)
        {
            return EtlActivityResult.Failure("No export format provider is available.");
        }

        var format = formatProvider.GetFormat(Format);

        if (format == null)
        {
            return EtlActivityResult.Failure($"The export format '{Format}' is not registered.");
        }

        var fileName = ResolveFileName(format);
        var recordsProcessed = 0;

        try
        {
            using var stream = new MemoryStream();
            await format.WriteAsync(
                CountRecordsAsync(data, () => recordsProcessed++, context.CancellationToken),
                stream,
                context.CancellationToken);
            stream.Position = 0;

            context.IncrementRecordsProcessed(recordsProcessed);

            await WriteToDestinationAsync(context, fileName, stream, format);
        }
        catch (Exception ex)
        {
            var logger = context.ServiceProvider.GetService<ILogger<EtlFileExportLoad>>();

            if (logger != null && logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(ex, "The ETL '{ActivityName}' load failed while writing '{FileName}'.", Name, fileName);
            }

            return EtlActivityResult.Failure($"The '{DisplayText}' export failed for '{fileName}': {ex.Message}");
        }

        context.IncrementRecordsLoaded(recordsProcessed);

        return Outcomes("Done");
    }

    /// <summary>
    /// Writes the serialized content to the concrete destination.
    /// </summary>
    /// <param name="context">The current execution context.</param>
    /// <param name="fileName">The resolved destination file name.</param>
    /// <param name="content">A readable stream positioned at the beginning of the serialized content.</param>
    /// <param name="format">The format used to serialize the data.</param>
    protected abstract Task WriteToDestinationAsync(EtlExecutionContext context, string fileName, Stream content, IEtlExportFormat format);

    private static async IAsyncEnumerable<JsonObject> CountRecordsAsync(
        IAsyncEnumerable<JsonObject> records,
        Action increment,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var record in records.WithCancellation(cancellationToken))
        {
            increment();
            yield return record;
        }
    }

    private string ResolveFileName(IEtlExportFormat format)
    {
        var name = FileName;

        if (string.IsNullOrWhiteSpace(name))
        {
            return $"etl-export.{format.FileExtension}";
        }

        return name;
    }
}
