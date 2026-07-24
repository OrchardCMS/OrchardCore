using System.Text;
using System.Text.Json.Nodes;

namespace OrchardCore.DataOrchestrator.Exporting;

/// <summary>
/// Serializes records as a comma-separated values (CSV) file. The header row is built from the
/// union of every record's top-level property names.
/// </summary>
public sealed class CsvExportFormat : IEtlExportFormat
{
    public string Name => "Csv";

    public string DisplayText => "CSV";

    public string FileExtension => "csv";

    public string MimeType => "text/csv";

    public async Task WriteAsync(IAsyncEnumerable<JsonObject> records, Stream output, CancellationToken cancellationToken)
    {
        var rows = new List<JsonObject>();
        var headers = new List<string>();
        var headerSet = new HashSet<string>(StringComparer.Ordinal);

        await foreach (var record in records.WithCancellation(cancellationToken))
        {
            rows.Add(record);

            foreach (var property in record)
            {
                if (headerSet.Add(property.Key))
                {
                    headers.Add(property.Key);
                }
            }
        }

        await using var writer = new StreamWriter(output, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), leaveOpen: true);

        await writer.WriteLineAsync(string.Join(',', headers.Select(EscapeCsv)));

        foreach (var record in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var values = headers.Select(header => EscapeCsv(GetCellValue(record, header)));
            await writer.WriteLineAsync(string.Join(',', values));
        }

        await writer.FlushAsync(cancellationToken);
    }

    private static string GetCellValue(JsonObject record, string header)
    {
        if (record.TryGetPropertyValue(header, out var node) && node is not null)
        {
            return node.ToString();
        }

        return string.Empty;
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
