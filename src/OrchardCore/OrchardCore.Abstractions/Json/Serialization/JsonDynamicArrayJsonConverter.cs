using System.Text.Json.Dynamic;
using System.Text.Json.Nodes;

#nullable enable

namespace System.Text.Json.Serialization;

public class JsonDynamicArrayJsonConverter : JsonConverter<JsonDynamicArray>
{
    public override JsonDynamicArray? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException($"Deserializing a {nameof(JsonDynamicArray)} is not supported.");
    }

    public override void Write(Utf8JsonWriter writer, JsonDynamicArray value, JsonSerializerOptions options)
    {
        ((JsonArray)value).WriteTo(writer, options);
    }
}

