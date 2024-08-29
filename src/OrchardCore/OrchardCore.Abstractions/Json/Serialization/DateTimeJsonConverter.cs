using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrchardCore.Json.Serialization;

public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    public static readonly DateTimeJsonConverter Instance = new();

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert != typeof(DateTime))
        {
            throw new ArgumentException("Unexpected type to convert.", nameof(typeToConvert));
        }

        if (!reader.TryGetDateTime(out var value))
        {
            var stringValue = reader.GetString();
            if (DateTime.TryParse(stringValue, out value))
            {
                return value;
            }

            throw new JsonException($"Unable to convert \"{stringValue}\" to DateTime.");
        }

        return value;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
}
