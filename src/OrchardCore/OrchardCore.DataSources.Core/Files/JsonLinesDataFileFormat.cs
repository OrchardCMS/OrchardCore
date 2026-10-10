using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;

namespace OrchardCore.DataSources.Files;

/// <summary>
/// Writes and reads JSON Lines files, which hold one JSON object per line. They can be appended to and read line by
/// line, which suits large exports. Fields are inferred the same way as <see cref="JsonDataFileFormat"/>.
/// </summary>
public sealed class JsonLinesDataFileFormat : IDataFileFormat
{
    /// <summary>
    /// The technical name of the format.
    /// </summary>
    public const string FormatName = "jsonl";

    private static readonly byte[] _newLine = "\n"u8.ToArray();

    public JsonLinesDataFileFormat(IStringLocalizer<JsonLinesDataFileFormat> localizer)
    {
        DisplayName = localizer["JSON Lines"];
    }

    public string Name => FormatName;

    public LocalizedString DisplayName { get; }

    public string Extension => ".jsonl";

    public string ContentType => "application/jsonl";

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

        await using var writer = new Utf8JsonWriter(stream);

        await foreach (var batch in batches.WithCancellation(cancellationToken))
        {
            foreach (var row in batch.Rows)
            {
                JsonDataRecords.WriteObject(writer, fields, row);
                await writer.FlushAsync(cancellationToken);
                writer.Reset();
                await stream.WriteAsync(_newLine, cancellationToken);
                count++;
            }
        }

        await stream.FlushAsync(cancellationToken);

        return count;
    }

    public IAsyncEnumerable<DataBatch> ReadAsync(
        Stream stream,
        DataFileOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        return JsonDataRecords.ToBatchesAsync(ReadObjectsAsync(stream, cancellationToken), options?.BatchSize ?? DataSourceQuery.DefaultBatchSize, cancellationToken);
    }

    private static async IAsyncEnumerable<JsonObject> ReadObjectsAsync(Stream stream, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (JsonNode.Parse(line) is JsonObject item)
            {
                yield return item;
            }
            else
            {
                throw new JsonException("Each line of a JSON Lines file must hold a JSON object.");
            }
        }
    }
}
