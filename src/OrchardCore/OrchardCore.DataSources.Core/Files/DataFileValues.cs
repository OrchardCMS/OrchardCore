using System.Globalization;

namespace OrchardCore.DataSources.Files;

/// <summary>
/// Formats and infers the values of data files.
/// </summary>
public static class DataFileValues
{
    /// <summary>
    /// Formats a value as invariant text for a text based file: dates as <c>yyyy-MM-dd</c>, date-times in ISO 8601 UTC,
    /// numbers without grouping, and booleans as <c>true</c> or <c>false</c>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="type">The type of the field the value belongs to.</param>
    /// <returns>The text, or <see langword="null"/> for a missing value.</returns>
    public static string ToText(object value, DataFieldType type)
    {
        if (value is null)
        {
            return null;
        }

        return type switch
        {
            DataFieldType.Date when DataValues.Coerce(value, DataFieldType.Date) is DateTime date
                => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DataFieldType.DateTime when DataValues.Coerce(value, DataFieldType.DateTime) is DateTime dateTime
                => ToUtc(dateTime).ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'", CultureInfo.InvariantCulture),
            _ => DataValues.ToText(value),
        };
    }

    /// <summary>
    /// Infers the type of a column from the values read so far: the common type of its values, or text when they
    /// disagree. Integers and decimals combine into decimals, and dates and date-times into date-times.
    /// </summary>
    /// <param name="types">The types of the values of the column, missing values excluded.</param>
    /// <returns>The inferred type.</returns>
    public static DataFieldType InferType(IEnumerable<DataFieldType> types)
    {
        ArgumentNullException.ThrowIfNull(types);

        DataFieldType? result = null;

        foreach (var type in types)
        {
            if (result is null || result == type)
            {
                result = type;

                continue;
            }

            if (DataValues.IsNumeric(result.Value) && DataValues.IsNumeric(type))
            {
                result = DataFieldType.Decimal;

                continue;
            }

            if (DataValues.IsTemporal(result.Value) && DataValues.IsTemporal(type))
            {
                result = DataFieldType.DateTime;

                continue;
            }

            return DataFieldType.Text;
        }

        return result ?? DataFieldType.Text;
    }

    /// <summary>
    /// Makes column names unique and non-empty: an empty name becomes <c>ColumnN</c>, where N is the position of the
    /// column, and a repeated name gets the next free number appended.
    /// </summary>
    /// <param name="names">The column names as read.</param>
    /// <returns>The unique names.</returns>
    public static IReadOnlyList<string> UniqueNames(IEnumerable<string> names)
    {
        ArgumentNullException.ThrowIfNull(names);

        var result = new List<string>();
        var used = new HashSet<string>(StringComparer.Ordinal);
        var position = 0;

        foreach (var raw in names)
        {
            position++;

            var name = string.IsNullOrWhiteSpace(raw) ? $"Column{position}" : raw.Trim();
            var candidate = name;
            var suffix = 2;

            while (!used.Add(candidate))
            {
                candidate = name + suffix.ToString(CultureInfo.InvariantCulture);
                suffix++;
            }

            result.Add(candidate);
        }

        return result;
    }

    private static DateTime ToUtc(DateTime value)
        => value.Kind switch
        {
            DateTimeKind.Local => value.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            _ => value,
        };
}
