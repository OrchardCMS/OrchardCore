namespace OrchardCore.DataSources;

/// <summary>
/// Identifies how a filter compares a field with its values.
/// </summary>
public enum DataFilterOperator
{
    /// <summary>
    /// The value equals the first filter value.
    /// </summary>
    Equals,

    /// <summary>
    /// The value differs from the first filter value.
    /// </summary>
    NotEquals,

    /// <summary>
    /// The text contains the first filter value, ignoring case.
    /// </summary>
    Contains,

    /// <summary>
    /// The text does not contain the first filter value, ignoring case.
    /// </summary>
    NotContains,

    /// <summary>
    /// The text starts with the first filter value, ignoring case.
    /// </summary>
    StartsWith,

    /// <summary>
    /// The text ends with the first filter value, ignoring case.
    /// </summary>
    EndsWith,

    /// <summary>
    /// The value is greater than the first filter value.
    /// </summary>
    GreaterThan,

    /// <summary>
    /// The value is greater than or equal to the first filter value.
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// The value is less than the first filter value.
    /// </summary>
    LessThan,

    /// <summary>
    /// The value is less than or equal to the first filter value.
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// The value lies between the first and second filter values, inclusive. An empty bound is open.
    /// </summary>
    Between,

    /// <summary>
    /// The value equals any of the filter values.
    /// </summary>
    In,

    /// <summary>
    /// The value equals none of the filter values.
    /// </summary>
    NotIn,

    /// <summary>
    /// The value is missing or an empty text.
    /// </summary>
    IsEmpty,

    /// <summary>
    /// The value is present and is not an empty text.
    /// </summary>
    IsNotEmpty,

    /// <summary>
    /// The date or date-time falls within the last N days, counting today, where N is the first filter value.
    /// </summary>
    InLastDays,

    /// <summary>
    /// The date or date-time falls within the next N days, counting today, where N is the first filter value.
    /// </summary>
    InNextDays,
}
