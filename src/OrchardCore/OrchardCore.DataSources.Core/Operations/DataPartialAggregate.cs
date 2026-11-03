namespace OrchardCore.DataSources.Operations;

/// <summary>
/// The aggregates of one field over a group of rows that a data source grouped itself. A consumer keeps one per group
/// in place of the rows' values, and <see cref="DataAggregations"/> merges them, so totals and regroupings stay exact.
/// </summary>
public sealed class DataPartialAggregate
{
    /// <summary>
    /// Gets or sets the number of rows with a value.
    /// </summary>
    public long Count { get; set; }

    /// <summary>
    /// Gets or sets the sum of the values, or <see langword="null"/>.
    /// </summary>
    public object Sum { get; set; }

    /// <summary>
    /// Gets or sets the smallest value, or <see langword="null"/>.
    /// </summary>
    public object Min { get; set; }

    /// <summary>
    /// Gets or sets the largest value, or <see langword="null"/>.
    /// </summary>
    public object Max { get; set; }
}
