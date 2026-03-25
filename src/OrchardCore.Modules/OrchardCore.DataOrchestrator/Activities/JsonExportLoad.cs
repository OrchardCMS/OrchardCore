using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.DataOrchestrator.Models;
using OrchardCore.FileStorage;
using OrchardCore.Media;

namespace OrchardCore.DataOrchestrator.Activities;

/// <summary>
/// Exports pipeline data as a JSON array to a file in the media file store.
/// </summary>
public sealed class JsonExportLoad : EtlLoadActivity
{
    public override string Name => nameof(JsonExportLoad);

    public override string DisplayText => "JSON Export";

    public string FileName
    {
        get => GetProperty(() => "etl-export.json");
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

        var logger = context.ServiceProvider.GetRequiredService<ILogger<JsonExportLoad>>();
        var mediaFileStore = context.ServiceProvider.GetService<IMediaFileStore>();

        if (mediaFileStore is null)
        {
            return EtlActivityResult.Failure("No IMediaFileStore service is available for JSON export.");
        }

        try
        {
            var records = new JsonArray();
            var loaded = 0;

            await foreach (var record in data.WithCancellation(context.CancellationToken))
            {
                records.Add(record.DeepClone());
                loaded++;
            }

            var fileName = mediaFileStore.NormalizePath(FileName);
            var jsonString = records.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            var bytes = Encoding.UTF8.GetBytes(jsonString);

            using var stream = new MemoryStream(bytes);
            await mediaFileStore.CreateFileFromStreamAsync(fileName, stream, overwrite: true);

            logger.LogInformation("ETL JSON export wrote {Count} records to '{FileName}'.", loaded, fileName);

            return Outcomes("Done");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ETL JSON export failed for '{FileName}'.", FileName);
            return EtlActivityResult.Failure($"JSON export failed for '{FileName}': {ex.Message}");
        }
    }
}
