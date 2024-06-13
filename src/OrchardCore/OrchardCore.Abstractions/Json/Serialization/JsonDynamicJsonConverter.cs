using System.Text.Json.Dynamic;

#nullable enable

namespace System.Text.Json.Serialization;

public sealed class JsonDynamicJsonConverter<T> : JsonConverter<T> where T : JsonDynamicBase
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException($"Deserializing a {typeof(T).Name} is not supported.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value.Node != null)
        {
            value.Node.WriteTo(writer, options);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
