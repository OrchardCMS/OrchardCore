using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;

namespace OrchardCore.DataSources.Files;

/// <summary>
/// Writes and reads JSON files that hold an array of objects, one per row. When reading, the fields and their types
/// are inferred from the objects of the first batch; properties that first appear later are ignored, and nested
/// objects and arrays are read as JSON text.
/// </summary>
public sealed class JsonDataFileFormat : IDataFileFormat
{
    /// <summary>
    /// The technical name of the format.
    /// </summary>
    public const string FormatName = "json";

    public JsonDataFileFormat(IStringLocalizer<JsonDataFileFormat> localizer)
    {
        DisplayName = localizer["JSON"];
    }

    public string Name => FormatName;

    public LocalizedString DisplayName { get; }

    public string Extension => ".json";

    public string ContentType => "application/json";

    public async Task<long> WriteAsync(
        Stream stream,
        IReadOnlyList<DataField> fields,
        IAsyncEnumerable<DataBatch> batches,
        DataFileOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(fields);
        ArgumentNullException.ThrowIfNull(batches);

        var count = 0L;

        await using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = options?.Indented ?? false });

        writer.WriteStartArray();

        await foreach (var batch in batches.WithCancellation(cancellationToken))
        {
            foreach (var row in batch.Rows)
            {
                JsonDataRecords.WriteObject(writer, fields, row);
                count++;
            }

            await writer.FlushAsync(cancellationToken);
        }

        writer.WriteEndArray();
        await writer.FlushAsync(cancellationToken);

        return count;
    }

    public IAsyncEnumerable<DataBatch> ReadAsync(
        Stream stream,
        DataFileOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var objects = JsonSerializer.DeserializeAsyncEnumerable<JsonObject>(stream, cancellationToken: cancellationToken);

        return JsonDataRecords.ToBatchesAsync(objects, options?.BatchSize ?? DataSourceQuery.DefaultBatchSize, cancellationToken);
    }
}
