using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrchardCore.Json.Serialization;

public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    public static readonly DateOnlyJsonConverter Instance = new();

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert != typeof(DateOnly))
        {
            throw new ArgumentException("Unexpected type to convert.", nameof(typeToConvert));
        }

        var stringValue = reader.GetString();

        if (DateOnly.TryParse(stringValue, out var timeSpan))
        {
            return timeSpan;
        }

        throw new JsonException($"Unable to convert '{stringValue}' to DateOnly.");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString("yyyy-MM-ddT", CultureInfo.InvariantCulture));
}
