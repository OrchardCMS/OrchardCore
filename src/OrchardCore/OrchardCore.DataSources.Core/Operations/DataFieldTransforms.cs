namespace OrchardCore.DataSources.Operations;

/// <summary>
/// A change applied to the values of a field before they are grouped or shown, such as keeping the month of a date.
/// </summary>
public enum DataFieldTransform
{
    /// <summary>
    /// The values are kept as they are.
    /// </summary>
    None,

    /// <summary>
    /// Text in upper case.
    /// </summary>
    Upper,

    /// <summary>
    /// Text in lower case.
    /// </summary>
    Lower,

    /// <summary>
    /// Text without leading and trailing spaces.
    /// </summary>
    Trim,

    /// <summary>
    /// The number of characters of text.
    /// </summary>
    Length,

    /// <summary>
    /// The year of a date, such as 2026.
    /// </summary>
    Year,

    /// <summary>
    /// The quarter of a date, from 1 to 4.
    /// </summary>
    Quarter,

    /// <summary>
    /// The first day of the month of a date, to group by month across years.
    /// </summary>
    Month,

    /// <summary>
    /// The Monday of the week of a date.
    /// </summary>
    Week,

    /// <summary>
    /// The day of a date, without its time.
    /// </summary>
    Day,

    /// <summary>
    /// The day of the week of a date, from 1 for Monday to 7 for Sunday.
    /// </summary>
    DayOfWeek,

    /// <summary>
    /// A date and time without its minutes and seconds.
    /// </summary>
    Hour,

    /// <summary>
    /// The month of a date, from 1 to 12, to compare months whatever their year.
    /// </summary>
    MonthOfYear,

    /// <summary>
    /// A number rounded to a whole number, away from zero.
    /// </summary>
    Round,
}

/// <summary>
/// Applies <see cref="DataFieldTransform"/> values. A value the transform doesn't apply to becomes missing.
/// </summary>
public static class DataFieldTransforms
{
    /// <summary>
    /// Tells whether a transform applies to the values of a type.
    /// </summary>
    /// <param name="transform">The transform.</param>
    /// <param name="type">The type of the values.</param>
    /// <returns><see langword="true"/> when the transform applies.</returns>
    public static bool Supports(DataFieldTransform transform, DataFieldType type)
    {
        return transform switch
        {
            DataFieldTransform.None => true,
            DataFieldTransform.Upper or DataFieldTransform.Lower or DataFieldTransform.Trim or DataFieldTransform.Length => type == DataFieldType.Text,
            DataFieldTransform.Hour => type == DataFieldType.DateTime,
            DataFieldTransform.Round => DataValues.IsNumeric(type),
            _ => DataValues.IsTemporal(type),
        };
    }

    /// <summary>
    /// Gets the type of the values a transform returns.
    /// </summary>
    /// <param name="transform">The transform.</param>
    /// <param name="type">The type of the values it reads.</param>
    /// <returns>The type of the values it returns.</returns>
    public static DataFieldType GetResultType(DataFieldTransform transform, DataFieldType type)
    {
        return transform switch
        {
            DataFieldTransform.None => type,
            DataFieldTransform.Upper or DataFieldTransform.Lower or DataFieldTransform.Trim => DataFieldType.Text,
            DataFieldTransform.Length or DataFieldTransform.Year or DataFieldTransform.Quarter or DataFieldTransform.DayOfWeek or DataFieldTransform.MonthOfYear or DataFieldTransform.Round => DataFieldType.Integer,
            DataFieldTransform.Month or DataFieldTransform.Week or DataFieldTransform.Day => DataFieldType.Date,
            DataFieldTransform.Hour => DataFieldType.DateTime,
            _ => type,
        };
    }

    /// <summary>
    /// Applies a transform to a value.
    /// </summary>
    /// <param name="transform">The transform.</param>
    /// <param name="value">The value.</param>
    /// <returns>The transformed value, or <see langword="null"/> when it is missing or the transform doesn't apply.</returns>
    public static object Apply(DataFieldTransform transform, object value)
    {
        if (transform == DataFieldTransform.None || value is null)
        {
            return value;
        }

        switch (transform)
        {
            case DataFieldTransform.Upper:
                return DataValues.ToText(value)?.ToUpperInvariant();

            case DataFieldTransform.Lower:
                return DataValues.ToText(value)?.ToLowerInvariant();

            case DataFieldTransform.Trim:
                return DataValues.ToText(value)?.Trim();

            case DataFieldTransform.Length:
                return DataValues.ToText(value) is string text ? (long)text.Length : null;

            case DataFieldTransform.Round:
                return DataValues.Coerce(value, DataFieldType.Decimal) is decimal number
                    ? (long)Math.Round(number, MidpointRounding.AwayFromZero)
                    : null;
        }

        if (DataValues.Coerce(value, DataFieldType.DateTime) is not DateTime date)
        {
            return null;
        }

        return transform switch
        {
            DataFieldTransform.Year => (long)date.Year,
            DataFieldTransform.Quarter => (long)(((date.Month - 1) / 3) + 1),
            DataFieldTransform.Month => new DateTime(date.Year, date.Month, 1, 0, 0, 0, date.Kind),
            DataFieldTransform.Week => date.Date.AddDays(-(((int)date.DayOfWeek + 6) % 7)),
            DataFieldTransform.Day => date.Date,
            DataFieldTransform.DayOfWeek => date.DayOfWeek == System.DayOfWeek.Sunday ? 7L : (long)date.DayOfWeek,
            DataFieldTransform.Hour => new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0, date.Kind),
            DataFieldTransform.MonthOfYear => (long)date.Month,
            _ => null,
        };
    }
}
