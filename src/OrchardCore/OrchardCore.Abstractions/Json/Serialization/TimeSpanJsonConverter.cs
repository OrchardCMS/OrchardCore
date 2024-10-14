using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrchardCore.Json.Serialization;

public class TimeSpanJsonConverter : JsonConverter<TimeSpan>
{
    public static readonly TimeSpanJsonConverter Instance = new();

    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Unexpected token parsing TimeSpan. Expected a string, got '{reader.TokenType}'.");
        }

        var stringValue = reader.GetString();

        if (TimeSpan.TryParse(stringValue, out var timeSpan))
        {
            return timeSpan;
        }

        throw new JsonException($"Unable to convert '{stringValue}' to TimeSpan.");
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
