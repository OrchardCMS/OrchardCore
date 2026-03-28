using System.Text.Json;
using System.Text.Json.Serialization;
using OrchardCore.Search.OpenSearch.Core.Models;
using OrchardCore.Search.OpenSearch.Models;

namespace OrchardCore.Search.OpenSearch.Core.Json;

internal sealed class OpenSearchIndexMetadataConverter : JsonConverter<OpenSearchIndexMetadata>
{
    public override void Write(Utf8JsonWriter writer, OpenSearchIndexMetadata value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteBoolean(nameof(value.StoreSourceData), value.StoreSourceData);
        writer.WriteString(nameof(value.AnalyzerName), value.AnalyzerName);

        writer.WritePropertyName(nameof(value.IndexMappings));
        JsonSerializer.Serialize(writer, value.IndexMappings, options);

        writer.WriteEndObject();
    }

    public override OpenSearchIndexMetadata Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var metadata = new OpenSearchIndexMetadata();

        if (root.TryGetProperty(nameof(OpenSearchIndexMetadata.StoreSourceData), out var storeSourceDataProperty))
        {
            metadata.StoreSourceData = storeSourceDataProperty.GetBoolean();
        }

        if (root.TryGetProperty(nameof(OpenSearchIndexMetadata.AnalyzerName), out var analyzerNameProperty))
        {
            metadata.AnalyzerName = analyzerNameProperty.GetString();
        }

        if (root.TryGetProperty(nameof(OpenSearchIndexMetadata.IndexMappings), out var indexMappingsProperty))
        {
            metadata.IndexMappings = JsonSerializer.Deserialize<OpenSearchIndexMap>(indexMappingsProperty.GetRawText(), options);
        }

        return metadata;
    }
}
