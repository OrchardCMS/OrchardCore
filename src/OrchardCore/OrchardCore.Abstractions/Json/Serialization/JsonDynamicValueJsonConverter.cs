using System.Text.Json.Dynamic;

#nullable enable

namespace System.Text.Json.Serialization;

public class JsonDynamicValueJsonConverter : JsonConverter<JsonDynamicValue>
{
    public override JsonDynamicValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException($"Deserializing a {nameof(JsonDynamicValue)} is not supported.");
    }

    public override void Write(Utf8JsonWriter writer, JsonDynamicValue value, JsonSerializerOptions options)
    {
        if (value.JsonValue != null)
        {
            value.JsonValue.WriteTo(writer, options);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}

