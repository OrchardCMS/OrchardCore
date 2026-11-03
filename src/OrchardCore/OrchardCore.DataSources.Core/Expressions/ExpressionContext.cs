namespace OrchardCore.DataSources.Expressions;

/// <summary>
/// The state a compiled formula reads while it is evaluated: the current row for a row-level formula, or for an
/// aggregate formula, the rows of the current group or the results of its aggregates.
/// </summary>
public sealed class ExpressionContext
{
    /// <summary>
    /// Gets or sets the current row, laid out as the scope the formula was compiled against describes.
    /// </summary>
    public object[] Row { get; set; }

    /// <summary>
    /// Gets or sets the rows of the current group, read by aggregate functions.
    /// </summary>
    public IReadOnlyList<object[]> Group { get; set; }

    /// <summary>
    /// Gets or sets the results of the aggregate calls of an aggregate formula for the current group, in the order of
    /// <see cref="CompiledExpression.Aggregates"/>, computed as the rows arrived. When set, the formula reads them
    /// instead of computing its aggregates over <see cref="Group"/>.
    /// </summary>
    public IReadOnlyList<object> AggregateValues { get; set; }

    /// <summary>
    /// Gets or sets the current date and time in the tenant time zone, returned by <c>NOW()</c> and <c>TODAY()</c>.
    /// </summary>
    public DateTime Now { get; set; }

    /// <summary>
    /// Gets or sets the slot that holds how many rows each row stands for, when a data source grouped the rows itself;
    /// <see langword="null"/> when every row is one row.
    /// </summary>
    public int? WeightSlot { get; set; }
}
