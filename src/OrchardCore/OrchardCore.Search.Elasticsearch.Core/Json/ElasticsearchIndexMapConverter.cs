using System.Text.Json;
using System.Text.Json.Serialization;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Transport;
using OrchardCore.Search.Elasticsearch.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Json;

public sealed class ElasticsearchIndexMapConverter : JsonConverter<ElasticsearchIndexMap>
{
    internal readonly static Serializer _elasticsearchSerializer;

    private static readonly JsonWriterOptions _writerOptions = new JsonWriterOptions
    {
        SkipValidation = true,
    };

    static ElasticsearchIndexMapConverter()
    {
        var settings = new ElasticsearchClientSettings();

        _elasticsearchSerializer = new ElasticsearchClient(settings).RequestResponseSerializer;
    }

    public override ElasticsearchIndexMap Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var keyFieldName = root.GetProperty(nameof(ElasticsearchIndexMap.KeyFieldName)).GetString();

        var mappingProp = root.GetProperty(nameof(ElasticsearchIndexMap.Mapping));

        var mappingStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(mappingStream, _writerOptions))
        {
            mappingProp.WriteTo(writer);
        }
        mappingStream.Position = 0;

        // Deserialize Mapping using Elasticsearch internal serializer
        var mapping = _elasticsearchSerializer.Deserialize<TypeMapping>(mappingStream);

        return new ElasticsearchIndexMap
        {
            KeyFieldName = keyFieldName,
            Mapping = mapping,
        };
    }

    public override void Write(Utf8JsonWriter writer, ElasticsearchIndexMap value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString(nameof(ElasticsearchIndexMap.KeyFieldName), value.KeyFieldName);

        writer.WritePropertyName(nameof(ElasticsearchIndexMap.Mapping));

        using var stream = new MemoryStream();
        _elasticsearchSerializer.Serialize(value.Mapping, stream);
        stream.Position = 0;

        using var jsonDoc = JsonDocument.Parse(stream);
        jsonDoc.RootElement.WriteTo(writer);

        writer.WriteEndObject();
    }
}
