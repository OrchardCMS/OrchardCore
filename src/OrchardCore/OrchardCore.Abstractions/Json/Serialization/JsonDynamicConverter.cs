using System.Collections.Generic;
using System.Dynamic;

namespace System.Text.Json.Serialization;

public class JsonDynamicConverter : JsonConverter<object>
{
    public static readonly JsonDynamicConverter Instance = new();

    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.False:
                return false;
            case JsonTokenType.True:
                return true;
            case JsonTokenType.String:
                return reader.GetString();
            case JsonTokenType.Number:
                {
                    if (reader.TryGetInt32(out var i))
                        return i;
                    if (reader.TryGetInt64(out var l))
                        return l;
                    // BigInteger could be added here.
                    if (reader.TryGetDouble(out var d))
                        return d;

                    throw new JsonException("Cannot parse number");
                }
            case JsonTokenType.StartArray:
                {
                    var list = new List<object>();
                    while (reader.Read())
                    {
                        switch (reader.TokenType)
                        {
                            default:
                                list.Add(Read(ref reader, typeof(object), options));
                                break;
                            case JsonTokenType.EndArray:
                                return list;
                        }
                    }
                    throw new JsonException();
                }
            case JsonTokenType.StartObject:
                // IDictionary<string, object> dict = new ExpandoObject();
                var dict = new ExpandoObject();
                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.EndObject:
                            return dict;
                        case JsonTokenType.PropertyName:
                            var key = reader.GetString();
                            reader.Read();
                            dict.TryAdd(key, Read(ref reader, typeof(object), options));
                            break;
                        default:
                            throw new JsonException();
                    }
                }
                throw new JsonException();
            default:
                throw new JsonException(string.Format("Unknown token {0}", reader.TokenType));
        }
    }

    public override void Write(
        Utf8JsonWriter writer,
        object objectToWrite,
        JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, objectToWrite, objectToWrite.GetType(), options);
}
