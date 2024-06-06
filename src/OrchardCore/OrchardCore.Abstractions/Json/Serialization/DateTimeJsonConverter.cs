using System;
using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
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
    {
        // The "O" standard format length can vary (up to 33 bytes for the round-trip format).
        Span<byte> utf8Date = new byte[33];

        bool result = Utf8Formatter.TryFormat(value, utf8Date, out var bytesWritten, new StandardFormat('O'));
        Debug.Assert(result);

        writer.WriteStringValue(utf8Date.Slice(0, bytesWritten));
    }
}
