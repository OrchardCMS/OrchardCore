using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OrchardCore.DataSources;

/// <summary>
/// Converts raw values to the CLR type a <see cref="DataFieldType"/> requires, and compares typed values. Data
/// sources use it to normalize what they read; consumers use it for filters, joins, and sorting.
/// </summary>
public static class DataValues
{
    private static readonly string[] _dateFormats =
    [
        "yyyy-MM-dd",
        "yyyy-MM-ddTHH:mm",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-ddTHH:mm:ss.FFFFFFF",
        "yyyy-MM-ddTHH:mm:ssZ",
        "yyyy-MM-ddTHH:mm:ss.FFFFFFFZ",
        "yyyy-MM-ddTHH:mm:ssK",
        "yyyy-MM-ddTHH:mm:ss.FFFFFFFK",
        "yyyy-MM-dd HH:mm",
        "yyyy-MM-dd HH:mm:ss",
    ];

    /// <summary>
    /// Converts a raw value to the CLR type of <paramref name="dataType"/>. Text is parsed with the invariant culture.
    /// </summary>
    /// <param name="value">The raw value, which may be a JSON node or element.</param>
    /// <param name="dataType">The target data type.</param>
    /// <returns>The converted value, or <see langword="null"/> when the value is missing or cannot be converted.</returns>
    public static object Coerce(object value, DataFieldType dataType)
    {
        value = Unwrap(value);

        if (value is null)
        {
            return null;
        }

        return dataType switch
        {
            DataFieldType.Text => ToText(value),
            DataFieldType.Integer => ToInteger(value),
            DataFieldType.Decimal => ToDecimal(value),
            DataFieldType.Boolean => ToBoolean(value),
            DataFieldType.Date => ToDateTime(value)?.Date,
            DataFieldType.DateTime => ToDateTime(value),
            _ => null,
        };
    }

    /// <summary>
    /// Infers the data type of a CLR value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The inferred data type; <see cref="DataFieldType.Text"/> for anything that is not a number, boolean, or date.</returns>
    public static DataFieldType InferType(object value)
    {
        return Unwrap(value) switch
        {
            bool => DataFieldType.Boolean,
            byte or sbyte or short or ushort or int or uint or long or ulong => DataFieldType.Integer,
            float or double or decimal => DataFieldType.Decimal,
            DateTime or DateTimeOffset => DataFieldType.DateTime,
            DateOnly => DataFieldType.Date,
            _ => DataFieldType.Text,
        };
    }

    /// <summary>
    /// Determines whether a data type is numeric.
    /// </summary>
    /// <param name="dataType">The data type.</param>
    /// <returns><see langword="true"/> for <see cref="DataFieldType.Integer"/> and <see cref="DataFieldType.Decimal"/>.</returns>
    public static bool IsNumeric(DataFieldType dataType)
    {
        return dataType is DataFieldType.Integer or DataFieldType.Decimal;
    }

    /// <summary>
    /// Determines whether a data type holds dates.
    /// </summary>
    /// <param name="dataType">The data type.</param>
    /// <returns><see langword="true"/> for <see cref="DataFieldType.Date"/> and <see cref="DataFieldType.DateTime"/>.</returns>
    public static bool IsTemporal(DataFieldType dataType)
    {
        return dataType is DataFieldType.Date or DataFieldType.DateTime;
    }

    /// <summary>
    /// Determines whether a value counts as empty: missing, or a text that is empty or white space.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns><see langword="true"/> when the value is empty.</returns>
    public static bool IsEmpty(object value)
    {
        return value is null || (value is string text && string.IsNullOrWhiteSpace(text));
    }

    /// <summary>
    /// Compares two typed values. Numbers compare by value whatever their CLR type, text compares ordinally ignoring
    /// case, and a missing value sorts before any other value.
    /// </summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns>A negative number, zero, or a positive number.</returns>
    public static int Compare(object left, object right)
    {
        if (left is null)
        {
            return right is null ? 0 : -1;
        }

        if (right is null)
        {
            return 1;
        }

        if (IsNumber(left) && IsNumber(right))
        {
            return ToDecimal(left).GetValueOrDefault().CompareTo(ToDecimal(right).GetValueOrDefault());
        }

        if (left is DateTime leftDate && right is DateTime rightDate)
        {
            return leftDate.CompareTo(rightDate);
        }

        if (left is bool leftBool && right is bool rightBool)
        {
            return leftBool.CompareTo(rightBool);
        }

        return string.Compare(ToText(left), ToText(right), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether two typed values are equal under the rules of <see cref="Compare(object, object)"/>.
    /// </summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns><see langword="true"/> when the values are equal.</returns>
    public static bool AreEqual(object left, object right)
    {
        return Compare(left, right) == 0;
    }

    /// <summary>
    /// Builds a key that is equal for values that <see cref="AreEqual(object, object)"/> treats as equal, so values can
    /// be grouped and joined through a dictionary.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The key.</returns>
    public static string ToKey(object value)
    {
        return value switch
        {
            null => "\0",
            string text => "s:" + text.ToUpperInvariant(),
            bool flag => flag ? "b:1" : "b:0",
            DateTime date => "d:" + date.Ticks.ToString(CultureInfo.InvariantCulture),
            _ when IsNumber(value) => "n:" + ToDecimal(value).GetValueOrDefault().ToString("G29", CultureInfo.InvariantCulture),
            _ => "s:" + ToText(value).ToUpperInvariant(),
        };
    }

    /// <summary>
    /// Converts a value to text using the invariant culture. Dates use the round-trip pattern.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The text, or <see langword="null"/> when the value is missing.</returns>
    public static string ToText(object value)
    {
        value = Unwrap(value);

        return value switch
        {
            null => null,
            string text => text,
            bool flag => flag ? "true" : "false",
            DateTime date => date.TimeOfDay == TimeSpan.Zero
                ? date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                : date.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
            DateTimeOffset offset => offset.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
            DateOnly dateOnly => dateOnly.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            decimal number => number.ToString("G29", CultureInfo.InvariantCulture),
            double number => number.ToString("R", CultureInfo.InvariantCulture),
            float number => number.ToString("R", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString(),
        };
    }

    private static object Unwrap(object value)
    {
        return value switch
        {
            JsonValue jsonValue => UnwrapElement(ToElement(jsonValue)),
            JsonElement element => UnwrapElement(element),
            JsonNode node => node.ToJsonString(),
            _ => value,
        };
    }

    /// <summary>
    /// Reads a JSON value as a <see cref="JsonElement"/>, whether it was parsed from JSON text or created from a CLR
    /// value.
    /// </summary>
    /// <param name="value">The JSON value.</param>
    /// <returns>The element.</returns>
    public static JsonElement ToElement(JsonValue value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.TryGetValue<JsonElement>(out var element)
            ? element
            : JsonSerializer.SerializeToElement(value);
    }

    private static object UnwrapElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var whole)
                ? whole
                : element.TryGetDecimal(out var number) ? number : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            _ => element.GetRawText(),
        };
    }

    private static bool IsNumber(object value)
    {
        return value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal;
    }

    private static long? ToInteger(object value)
    {
        var number = ToDecimal(value);

        if (!number.HasValue)
        {
            return null;
        }

        var truncated = decimal.Truncate(number.Value);

        if (truncated < long.MinValue || truncated > long.MaxValue)
        {
            return null;
        }

        return (long)truncated;
    }

    private static decimal? ToDecimal(object value)
    {
        try
        {
            return value switch
            {
                decimal number => number,
                double number => double.IsFinite(number) ? (decimal)number : null,
                float number => float.IsFinite(number) ? (decimal)number : null,
                bool flag => flag ? 1 : 0,
                string text => decimal.TryParse(text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) ? parsed : null,
                IConvertible convertible when IsNumber(value) => convertible.ToDecimal(CultureInfo.InvariantCulture),
                _ => null,
            };
        }
        catch (OverflowException)
        {
            return null;
        }
    }

    private static bool? ToBoolean(object value)
    {
        return value switch
        {
            bool flag => flag,
            string text => ParseBoolean(text),
            _ when IsNumber(value) => ToDecimal(value) != 0,
            _ => null,
        };
    }

    private static bool? ParseBoolean(string text)
    {
        var trimmed = text.Trim();

        if (bool.TryParse(trimmed, out var flag))
        {
            return flag;
        }

        return trimmed.ToLowerInvariant() switch
        {
            "1" or "yes" or "y" or "on" => true,
            "0" or "no" or "n" or "off" => false,
            _ => null,
        };
    }

    private static DateTime? ToDateTime(object value)
    {
        return value switch
        {
            DateTime date => date,
            DateTimeOffset offset => offset.UtcDateTime,
            DateOnly dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
            string text => ParseDateTime(text),
            _ => null,
        };
    }

    private static DateTime? ParseDateTime(string text)
    {
        var trimmed = text.Trim();

        if (trimmed.Length == 0)
        {
            return null;
        }

        if (DateTime.TryParseExact(trimmed, _dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AllowWhiteSpaces, out var exact))
        {
            return exact;
        }

        if (DateTime.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var parsed))
        {
            return parsed;
        }

        return null;
    }
}
