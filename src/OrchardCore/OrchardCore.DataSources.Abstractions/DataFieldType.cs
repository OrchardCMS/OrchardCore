namespace OrchardCore.DataSources;

/// <summary>
/// Identifies the logical type of a data field. Every value a data source returns must use the CLR type that matches
/// its field's type: <see cref="string"/> for <see cref="Text"/>, <see cref="long"/> for <see cref="Integer"/>,
/// <see cref="decimal"/> for <see cref="Decimal"/>, <see cref="bool"/> for <see cref="Boolean"/>, and
/// <see cref="System.DateTime"/> for <see cref="Date"/> and <see cref="DateTime"/>. Use
/// <see cref="DataValues.Coerce(object, DataFieldType)"/> to convert raw values.
/// </summary>
public enum DataFieldType
{
    /// <summary>
    /// A text value.
    /// </summary>
    Text,

    /// <summary>
    /// A whole number.
    /// </summary>
    Integer,

    /// <summary>
    /// A decimal number.
    /// </summary>
    Decimal,

    /// <summary>
    /// A true or false value.
    /// </summary>
    Boolean,

    /// <summary>
    /// A calendar date with no time of day and no time zone.
    /// </summary>
    Date,

    /// <summary>
    /// A point in time, in UTC.
    /// </summary>
    DateTime,
}
