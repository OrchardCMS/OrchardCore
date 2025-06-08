using System.Text.Json;
using System.Text.Json.Serialization;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Transport;
using Elastic.Transport.Extensions;
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

        // Write KeyFieldName using standard serialization
        writer.WriteString(nameof(ElasticsearchIndexMap.KeyFieldName), value.KeyFieldName);

        // Write Mapping using Elasticsearch serializer.
        writer.WritePropertyName(nameof(ElasticsearchIndexMap.Mapping));

        _elasticsearchSerializer.Serialize(value.Mapping, writer);

        writer.WriteEndObject();
    }
}
