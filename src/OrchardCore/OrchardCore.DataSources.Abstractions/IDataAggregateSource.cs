namespace OrchardCore.DataSources;

/// <summary>
/// A data source that can group a data set and aggregate its values itself, such as with a <c>GROUP BY</c> in its
/// database, so a consumer that only counts, sums or takes the smallest or largest values reads a few groups instead
/// of every row. It is optional: a consumer asks first, and reads the rows when the data source answers
/// <see langword="null"/>, so a data source may decline any query it can't answer exactly.
/// </summary>
public interface IDataAggregateSource
{
    /// <summary>
    /// Groups a data set and aggregates its values. The data source must check that the user of the context may read
    /// the data set, and answer <see langword="null"/> when it may not.
    /// </summary>
    /// <param name="query">The data set, what to group by, what to compute and which rows to include.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// One row per group: the values of the <see cref="DataAggregateQuery.Groups"/>, in order, followed by the values
    /// of the <see cref="DataAggregateQuery.Measures"/>, in order, with the CLR types of <see cref="DataFieldType"/>
    /// and dates in UTC. <see langword="null"/> when the data source can't answer the query exactly, such as for a
    /// field it can't group or aggregate, a condition it can't apply exactly, or more groups than
    /// <see cref="DataAggregateQuery.MaxGroups"/>.
    /// </returns>
    Task<IReadOnlyList<object[]>> AggregateAsync(DataAggregateQuery query, CancellationToken cancellationToken = default);
}
