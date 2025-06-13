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

        return new ElasticsearchIndexMetadata
        {
            StoreSourceData = root.GetProperty(nameof(ElasticsearchIndexMetadata.StoreSourceData)).GetBoolean(),
            AnalyzerName = root.GetProperty(nameof(ElasticsearchIndexMetadata.AnalyzerName)).GetString(),
            IndexMappings = JsonSerializer.Deserialize<ElasticsearchIndexMap>(root.GetProperty(nameof(ElasticsearchIndexMetadata.IndexMappings)).GetRawText(), options),
        };
    }
}
