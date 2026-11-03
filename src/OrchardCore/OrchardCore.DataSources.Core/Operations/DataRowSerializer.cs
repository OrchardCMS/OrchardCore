namespace OrchardCore.DataSources.Operations;

/// <summary>
/// Writes and reads rows in a compact binary form that keeps the type of each value. A value of a type it doesn't
/// know is written as text.
/// </summary>
internal static class DataRowSerializer
{
    private const byte Null = 0;
    private const byte Text = 1;
    private const byte Int64 = 2;
    private const byte Int32 = 3;
    private const byte Decimal = 4;
    private const byte Double = 5;
    private const byte Boolean = 6;
    private const byte DateTimeValue = 7;
    private const byte DateTimeOffsetValue = 8;
    private const byte GuidValue = 9;
    private const byte TimeSpanValue = 10;
    private const byte Single = 11;
    private const byte Int16 = 12;
    private const byte Byte = 13;
    private const byte PartialAggregate = 14;

    public static void Write(BinaryWriter writer, object[] row)
    {
        if (row is null)
        {
            writer.Write(-1);

            return;
        }

        writer.Write(row.Length);

        foreach (var value in row)
        {
            WriteValue(writer, value);
        }
    }

    public static object[] Read(BinaryReader reader)
    {
        var length = reader.ReadInt32();

        if (length < 0)
        {
            return null;
        }

        var row = new object[length];

        for (var index = 0; index < length; index++)
        {
            row[index] = ReadValue(reader);
        }

        return row;
    }

    private static void WriteValue(BinaryWriter writer, object value)
    {
        switch (value)
        {
            case null:
                writer.Write(Null);
                break;

            case string text:
                writer.Write(Text);
                writer.Write(text);
                break;

            case long number:
                writer.Write(Int64);
                writer.Write(number);
                break;

            case int number:
                writer.Write(Int32);
                writer.Write(number);
                break;

            case decimal number:
                writer.Write(Decimal);
                writer.Write(number);
                break;

            case double number:
                writer.Write(Double);
                writer.Write(number);
                break;

            case bool flag:
                writer.Write(Boolean);
                writer.Write(flag);
                break;

            case DateTime date:
                writer.Write(DateTimeValue);
                writer.Write(date.ToBinary());
                break;

            case DateTimeOffset date:
                writer.Write(DateTimeOffsetValue);
                writer.Write(date.Ticks);
                writer.Write(date.Offset.Ticks);
                break;

            case Guid guid:
                writer.Write(GuidValue);
                writer.Write(guid.ToByteArray());
                break;

            case TimeSpan span:
                writer.Write(TimeSpanValue);
                writer.Write(span.Ticks);
                break;

            case float number:
                writer.Write(Single);
                writer.Write(number);
                break;

            case short number:
                writer.Write(Int16);
                writer.Write(number);
                break;

            case byte number:
                writer.Write(Byte);
                writer.Write(number);
                break;

            case DataPartialAggregate part:
                writer.Write(PartialAggregate);
                writer.Write(part.Count);
                WriteValue(writer, part.Sum);
                WriteValue(writer, part.Min);
                WriteValue(writer, part.Max);
                break;

            default:
                writer.Write(Text);
                writer.Write(DataValues.ToText(value) ?? string.Empty);
                break;
        }
    }

    private static object ReadValue(BinaryReader reader)
    {
        var tag = reader.ReadByte();

        return tag switch
        {
            Null => null,
            Text => reader.ReadString(),
            Int64 => reader.ReadInt64(),
            Int32 => reader.ReadInt32(),
            Decimal => reader.ReadDecimal(),
            Double => reader.ReadDouble(),
            Boolean => reader.ReadBoolean(),
            DateTimeValue => DateTime.FromBinary(reader.ReadInt64()),
            DateTimeOffsetValue => new DateTimeOffset(reader.ReadInt64(), new TimeSpan(reader.ReadInt64())),
            GuidValue => new Guid(reader.ReadBytes(16)),
            TimeSpanValue => new TimeSpan(reader.ReadInt64()),
            Single => reader.ReadSingle(),
            Int16 => reader.ReadInt16(),
            Byte => reader.ReadByte(),
            PartialAggregate => new DataPartialAggregate
            {
                Count = reader.ReadInt64(),
                Sum = ReadValue(reader),
                Min = ReadValue(reader),
                Max = ReadValue(reader),
            },
            _ => throw new InvalidDataException($"The temporary file of rows holds an unknown value type {tag}."),
        };
    }
}
