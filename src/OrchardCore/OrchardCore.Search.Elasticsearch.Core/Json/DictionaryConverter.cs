using System.Text.Json;
using System.Text.Json.Serialization;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Transport.Extensions;

namespace OrchardCore.Search.Elasticsearch.Core.Json;

internal sealed class DictionaryConverter : JsonConverter<Dictionary<string, IProperty>>
{
    public override Dictionary<string, IProperty> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var dictionary = new Dictionary<string, IProperty>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return dictionary;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            var key = reader.GetString();
            reader.Read(); // Move to the value

            dictionary[key] = ElasticsearchSerializerOptions.ElasticsearchRequestResponseSerializer.Deserialize<IProperty>(ref reader);
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<string, IProperty> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (var kvp in value)
        {
            writer.WritePropertyName(kvp.Key);

            ElasticsearchSerializerOptions.ElasticsearchRequestResponseSerializer.Serialize(kvp.Value, writer);
        }

        writer.WriteEndObject();
    }
}
