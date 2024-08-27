using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrchardCore.Json.Serialization;

/// <summary>
/// JSON serializer for dictionaries that use <see cref="StringComparer.OrdinalIgnoreCase"/>.
/// </summary>
public sealed class CaseInsensitiveDictionaryConverter<TValue> : JsonConverter<Dictionary<string, TValue>>
{
    public override Dictionary<string, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, TValue>>(ref reader, options);
        return new(dictionary, StringComparer.OrdinalIgnoreCase);
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<string, TValue> value, JsonSerializerOptions options) => 
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
}
