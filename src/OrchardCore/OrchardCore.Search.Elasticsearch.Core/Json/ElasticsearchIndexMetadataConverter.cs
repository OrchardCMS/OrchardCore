using System.Text.Json;
using System.Text.Json.Serialization;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Json;

internal sealed class ElasticsearchIndexMetadataConverter : JsonConverter<ElasticsearchIndexMetadata>
{
    public override void Write(Utf8JsonWriter writer, ElasticsearchIndexMetadata value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteBoolean(nameof(value.StoreSourceData), value.StoreSourceData);
        writer.WriteString(nameof(value.AnalyzerName), value.AnalyzerName);

        writer.WritePropertyName(nameof(value.IndexMappings));
        JsonSerializer.Serialize(writer, value.IndexMappings, options);

        writer.WriteEndObject();
    }

    public override ElasticsearchIndexMetadata Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var metadata = new ElasticsearchIndexMetadata();

        if (root.TryGetProperty(nameof(ElasticsearchIndexMetadata.StoreSourceData), out var storeSourceDataProperty))
        {
            metadata.StoreSourceData = storeSourceDataProperty.GetBoolean();
        }

        if (root.TryGetProperty(nameof(ElasticsearchIndexMetadata.AnalyzerName), out var analyzerNameProperty))
        {
            metadata.AnalyzerName = analyzerNameProperty.GetString();
        }

        if (root.TryGetProperty(nameof(ElasticsearchIndexMetadata.IndexMappings), out var indexMappingsProperty))
        {
            metadata.IndexMappings = JsonSerializer.Deserialize<ElasticsearchIndexMap>(indexMappingsProperty.GetRawText(), options);
        }

        return metadata;
    }
}
