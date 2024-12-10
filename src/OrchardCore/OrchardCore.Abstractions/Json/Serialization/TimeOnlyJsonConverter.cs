using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrchardCore.Json.Serialization;

public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    public static readonly TimeOnlyJsonConverter Instance = new();

    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Unexpected token parsing TimeSpan. Expected a string, got '{reader.TokenType}'.");
        }

        var stringValue = reader.GetString();

        if (TimeOnly.TryParse(stringValue, out var timeSpan))
        {
            return timeSpan;
        }

        throw new JsonException($"Unable to convert '{stringValue}' to TimeOnly.");
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
