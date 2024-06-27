using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrchardCore.Queries;

public sealed class QueryJsonConverter : JsonConverter<Query>
{
    public static readonly QueryJsonConverter Instance = new();

    public override Query Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string source = null;
        string name = null;
        string schema = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case nameof(Query.Source):
                        source = reader.GetString();
                        break;
                    case nameof(Query.Name):
                        name = reader.GetString();
                        break;
                    case nameof(Query.Schema):
                        schema = reader.GetString();
                        break;
                    default:
                        break;
                }
            }
        }

        return new Query(source, name, schema);
    }

    public override void Write(Utf8JsonWriter writer, Query value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(nameof(Query.Source), value.Source);
        writer.WriteString(nameof(Query.Name), value.Name);
        writer.WriteString(nameof(Query.Schema), value.Schema);
        writer.WriteEndObject();
    }
}
