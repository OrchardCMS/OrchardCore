using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrchardCore.Json.Serialization;
public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    // ISO 8601 format with milliseconds and time zone
    private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss"; 

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Unexpected token parsing DateTime. Expected a string, got {reader.TokenType}.");
        }

        var stringValue = reader.GetString();
        if (DateTime.TryParse(stringValue, out var dateTime))
        {
            return dateTime;
        }

        throw new JsonException($"Unable to convert \"{stringValue}\" to DateTime.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(DateTimeFormat, CultureInfo.InvariantCulture));
    }
}
