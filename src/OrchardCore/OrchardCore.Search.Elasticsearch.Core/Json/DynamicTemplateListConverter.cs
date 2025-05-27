using System.Text.Json;
using System.Text.Json.Serialization;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Transport.Extensions;

namespace OrchardCore.Search.Elasticsearch.Core.Json;

internal sealed class DynamicTemplateListConverter : JsonConverter<IList<IDictionary<string, DynamicTemplate>>>
{
    public override IList<IDictionary<string, DynamicTemplate>> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var list = new List<IDictionary<string, DynamicTemplate>>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return list;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            var templateName = reader.GetString();
            reader.Read();

            var innerDict = new Dictionary<string, DynamicTemplate>
            {
                { templateName, ElasticsearchSerializerOptions.ElasticsearchRequestResponseSerializer.Deserialize<DynamicTemplate>(ref reader) },
            };

            list.Add(innerDict);
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, IList<IDictionary<string, DynamicTemplate>> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (var dict in value)
        {
            foreach (var kv in dict)
            {
                writer.WritePropertyName(kv.Key);

                ElasticsearchSerializerOptions.ElasticsearchRequestResponseSerializer.Serialize(kv.Value, writer);
            }
        }

        writer.WriteEndObject();
    }
}
