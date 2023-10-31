using System.Collections.Generic;
using System.Dynamic;

#nullable enable

namespace System.Text.Json.Serialization;

public class JsonDynamicConverter : JsonConverter<object>
{
    public static readonly JsonDynamicConverter Instance = new();

    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            return JsonSerializer.Deserialize(ref reader, typeToConvert, options);
        }

        var properties = new ExpandoObject();
        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.EndObject:
                    return properties;

                case JsonTokenType.PropertyName:
                    var key = reader.GetString();
                    if (key is not null)
                    {
                        reader.Read();
                        properties.TryAdd(key, Read(ref reader, typeof(object), options));
                    }
                    break;

                default:
                    throw new JsonException();
            }
        }

        throw new JsonException(string.Format("Unknown token {0}", reader.TokenType));
    }

    public override void Write(Utf8JsonWriter writer, object objectToWrite, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, objectToWrite, objectToWrite.GetType(), options);
}
