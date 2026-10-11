namespace OrchardCore.DataSources;

/// <summary>
/// Identifies how a column combines the values of a group of rows.
/// </summary>
public enum DataAggregate
{
    /// <summary>
    /// No aggregate; the column is a dimension.
    /// </summary>
    None,

    /// <summary>
    /// The number of rows with a value.
    /// </summary>
    Count,

    /// <summary>
    /// The number of distinct values.
    /// </summary>
    CountDistinct,

    /// <summary>
    /// The sum of the values.
    /// </summary>
    Sum,

    /// <summary>
    /// The average of the values.
    /// </summary>
    Average,

    /// <summary>
    /// The smallest value.
    /// </summary>
    Min,

    /// <summary>
    /// The largest value.
    /// </summary>
    Max,

    /// <summary>
    /// The middle value.
    /// </summary>
    Median,
}
