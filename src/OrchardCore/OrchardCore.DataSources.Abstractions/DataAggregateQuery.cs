namespace OrchardCore.DataSources;

/// <summary>
/// Describes how a consumer asks a data source to group the rows of one data set and aggregate their values. See
/// <see cref="IDataAggregateSource"/>.
/// </summary>
public sealed class DataAggregateQuery
{
    /// <summary>
    /// The most groups a data source returns when <see cref="MaxGroups"/> is not set.
    /// </summary>
    public const int DefaultMaxGroups = 10_000;

    /// <summary>
    /// Gets or sets the technical name of the data set to aggregate.
    /// </summary>
    public string DataSet { get; set; }

    /// <summary>
    /// Gets or sets the fields to group by, in the order of the first values of each result row. Without groups, every
    /// row belongs to one group, and an empty data set still has one.
    /// </summary>
    public IList<DataAggregateGroup> Groups { get; set; } = [];

    /// <summary>
    /// Gets or sets the values to compute for each group, in the order of the result values that follow the groups.
    /// </summary>
    public IList<DataAggregateMeasure> Measures { get; set; } = [];

    /// <summary>
    /// Gets or sets the conditions every row must meet. Unlike the conditions of a <see cref="DataSourceQuery"/>, which
    /// a consumer applies again, these must be applied exactly, or the data source must decline the query:
    /// <list type="bullet">
    /// <item><description>values compare exactly, with dates in UTC;</description></item>
    /// <item><description>text compares ignoring case;</description></item>
    /// <item><description><see cref="DataFilterOperator.NotEquals"/> and <see cref="DataFilterOperator.NotIn"/> keep
    /// the rows without a value.</description></item>
    /// </list>
    /// </summary>
    public IList<DataCondition> Conditions { get; set; } = [];

    /// <summary>
    /// Gets or sets the most groups the result may have. A data source with more groups answers
    /// <see langword="null"/>.
    /// </summary>
    public int MaxGroups { get; set; } = DefaultMaxGroups;

    /// <summary>
    /// Gets or sets the caller context of the query. The data source answers only with the rows its user may read.
    /// </summary>
    public DataSourceContext Context { get; set; } = new();
}

/// <summary>
/// A field a <see cref="DataAggregateQuery"/> groups by.
/// </summary>
public sealed class DataAggregateGroup
{
    /// <summary>
    /// Gets or sets the technical name of the field.
    /// </summary>
    public string Field { get; set; }

    /// <summary>
    /// Gets or sets the boundaries of the buckets that group the values of a date field, in ascending order, in UTC
    /// for a date-time field, or <see langword="null"/> to group by the values themselves. With boundaries, the value
    /// of a group is the zero-based index of its bucket, as a <see cref="long"/>: bucket <c>i</c> holds the values from
    /// <c>Boundaries[i]</c>, included, to <c>Boundaries[i + 1]</c>, excluded. A row without a value, or outside every
    /// bucket, belongs to the group whose value is <see langword="null"/>.
    /// </summary>
    public IList<DateTime> Boundaries { get; set; }
}

/// <summary>
/// A value a <see cref="DataAggregateQuery"/> computes for each group.
/// </summary>
public sealed class DataAggregateMeasure
{
    /// <summary>
    /// Gets or sets what to compute: <see cref="DataAggregate.Count"/>, <see cref="DataAggregate.Sum"/>,
    /// <see cref="DataAggregate.Min"/> or <see cref="DataAggregate.Max"/>. These are the aggregates a consumer can
    /// combine across groups, such as for totals, and derive others from, such as an average from a sum and a count.
    /// A data source answers <see langword="null"/> to any other aggregate.
    /// </summary>
    public DataAggregate Function { get; set; }

    /// <summary>
    /// Gets or sets the technical name of the field to aggregate, or <see langword="null"/> to count the rows of each
    /// group with <see cref="DataAggregate.Count"/>. A count of a field counts the rows with a value.
    /// </summary>
    public string Field { get; set; }
}
