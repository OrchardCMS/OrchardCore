using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OrchardCore.DataOrchestrator.Exporting;

/// <summary>
/// Serializes records as an indented JSON array.
/// </summary>
public sealed class JsonExportFormat : IEtlExportFormat
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
    };

    public string Name => "Json";

    public string DisplayText => "JSON";

    public string FileExtension => "json";

    public string MimeType => "application/json";

    public async Task WriteAsync(IReadOnlyList<JsonObject> records, Stream output, CancellationToken cancellationToken)
    {
        var array = new JsonArray();

        foreach (var record in records)
        {
            array.Add(record.DeepClone());
        }

        var json = array.ToJsonString(_options);
        var bytes = Encoding.UTF8.GetBytes(json);

        await output.WriteAsync(bytes, cancellationToken);
    }
}
