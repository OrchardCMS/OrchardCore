#nullable enable

namespace System.Text.Json.Serialization;

public class DynamicJsonConverter : JsonConverter<object>
{
    public static readonly DynamicJsonConverter Instance = new();

    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                if (reader.TryGetInt32(out var intObject))
                {
                    return intObject;
                }

                if (reader.TryGetInt64(out var longObject))
                {
                    return longObject;
                }

                if (reader.TryGetDecimal(out var decimalValue))
                {
                    return decimalValue;
                }

                if (JConvert.TryGetBigInteger(ref reader, out var bigInteger))
                {
                    return bigInteger;
                }

                if (reader.TryGetDouble(out var doubleObject))
                {
                    return doubleObject;
                }

                throw new JsonException("Cannot parse number");

            case JsonTokenType.StartArray:
                var list = new List<object?>();
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

                throw new JsonException("Cannot parse array.");

            case JsonTokenType.StartObject:
                var dictionary = new Dictionary<string, object?>();
                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.EndObject:
                            return dictionary;

                        case JsonTokenType.PropertyName:
                            var key = reader.GetString();
                            reader.Read();
                            if (key is not null)
                            {
                                dictionary[key] = Read(ref reader, typeof(object), options);
                            }

                            break;

                        default:
                            throw new JsonException("Cannot parse object.");
                    }
                }

                throw new JsonException();

            default:
                throw new JsonException(string.Format("Unknown token type {0}", reader.TokenType));
        }
    }

    public override void Write(
        Utf8JsonWriter writer,
        object objectToWrite,
        JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, objectToWrite, objectToWrite.GetType(), options);
}
