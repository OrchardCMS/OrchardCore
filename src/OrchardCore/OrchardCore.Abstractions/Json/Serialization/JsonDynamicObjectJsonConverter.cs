using System.Text.Json.Dynamic;
using System.Text.Json.Nodes;

#nullable enable

namespace System.Text.Json.Serialization;

public class JsonDynamicObjectJsonConverter : JsonConverter<JsonDynamicObject>
{
    public override JsonDynamicObject? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException($"Deserializing a {nameof(JsonDynamicObject)} is not supported.");
    }

    public override void Write(Utf8JsonWriter writer, JsonDynamicObject value, JsonSerializerOptions options)
    {
        ((JsonObject)value).WriteTo(writer, options);
    }
}

