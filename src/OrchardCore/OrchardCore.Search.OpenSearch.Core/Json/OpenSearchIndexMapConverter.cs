using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenSearch.Client;
using OpenSearch.Net;
using OrchardCore.Search.OpenSearch.Models;

namespace OrchardCore.Search.OpenSearch.Core.Json;

public sealed class OpenSearchIndexMapConverter : JsonConverter<OpenSearchIndexMap>
{
    internal static readonly IOpenSearchSerializer _openSearchSerializer;

    private static readonly JsonWriterOptions _writerOptions = new JsonWriterOptions
    {
        SkipValidation = true,
    };

    static OpenSearchIndexMapConverter()
    {
        var settings = new ConnectionSettings();
        _openSearchSerializer = new OpenSearchClient(settings).RequestResponseSerializer;
    }

    public override OpenSearchIndexMap Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var keyFieldName = root.GetProperty(nameof(OpenSearchIndexMap.KeyFieldName)).GetString();

        var mappingProp = root.GetProperty(nameof(OpenSearchIndexMap.Mapping));

        var mappingStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(mappingStream, _writerOptions))
        {
            mappingProp.WriteTo(writer);
        }
        mappingStream.Position = 0;

        var mapping = _openSearchSerializer.Deserialize<TypeMapping>(mappingStream);

        return new OpenSearchIndexMap
        {
            KeyFieldName = keyFieldName,
            Mapping = mapping,
        };
    }

    public override void Write(Utf8JsonWriter writer, OpenSearchIndexMap value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString(nameof(OpenSearchIndexMap.KeyFieldName), value.KeyFieldName);

        writer.WritePropertyName(nameof(OpenSearchIndexMap.Mapping));

        using var stream = new MemoryStream();
        _openSearchSerializer.Serialize(value.Mapping, stream);
        stream.Position = 0;

        using var jsonDoc = JsonDocument.Parse(stream);
        jsonDoc.RootElement.WriteTo(writer);

        writer.WriteEndObject();
    }
}
