using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrchardCore.Json.Serialization;
public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (DateTime.TryParse(reader.GetString(), out DateTime value))
        {
            return value;
        }

        throw new FormatException("Invalid date format.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
}
