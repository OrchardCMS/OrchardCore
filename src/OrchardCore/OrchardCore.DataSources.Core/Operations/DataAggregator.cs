using System.Runtime.CompilerServices;

namespace OrchardCore.DataSources.Operations;

/// <summary>
/// A value computed for each group of rows.
/// </summary>
/// <param name="Function">The aggregate function.</param>
/// <param name="FieldIndex">The position of the field the function reads; <c>-1</c> counts the rows.</param>
public sealed record DataMeasure(DataAggregate Function, int FieldIndex);

/// <summary>
/// Groups rows by the values of fields and computes aggregates for each group, with a bounded amount of memory.
/// </summary>
public static class DataAggregator
{
    // The memory a group takes besides its key and its values, and an aggregate besides the values it keeps.
    private const int GroupOverhead = 96;
    private const int MeasureOverhead = 64;

    /// <summary>
    /// Groups rows and computes the measures of each group, like a SQL <c>GROUP BY</c>. The groups stay in memory up to
    /// <see cref="DataSpillOptions.MemoryLimit"/>; the rows of the groups that don't fit are written to a temporary
    /// file and grouped once the others are done. Without group fields, every row belongs to one group, and an empty
    /// input still has one group.
    /// </summary>
    /// <param name="rows">The rows.</param>
    /// <param name="groupIndexes">The positions of the fields whose values define the groups, under the rules of
    /// <see cref="DataKeys"/>.</param>
    /// <param name="measures">The values computed for each group.</param>
    /// <param name="options">The memory limit and the directory of the temporary files.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A row per group, in the order the groups first appear: the values of the group fields, then the
    /// value of each measure.</returns>
    public static async IAsyncEnumerable<object[]> AggregateAsync(
        IAsyncEnumerable<object[]> rows,
        IReadOnlyList<int> groupIndexes,
        IReadOnlyList<DataMeasure> measures,
        DataSpillOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(groupIndexes);
        ArgumentNullException.ThrowIfNull(measures);
        ArgumentNullException.ThrowIfNull(options);

        var source = rows;
        DataRowBuffer previous = null;
        var any = false;

        try
        {
            // Each pass computes the groups that fit in memory; the other groups all appear after the last group the
            // pass computed, so the next pass continues the order.
            while (true)
            {
                var groups = new Dictionary<string, Group>(StringComparer.Ordinal);
                var order = new List<Group>();
                var size = 0L;
                DataRowBuffer remaining = null;

                await foreach (var row in source.WithCancellation(cancellationToken))
                {
                    any = true;

                    var key = DataKeys.Build(row, groupIndexes, ignoreCase: false);

                    if (!groups.TryGetValue(key, out var group))
                    {
                        if (remaining is not null)
                        {
                            remaining.Add(row);

                            continue;
                        }

                        var values = groupIndexes.Select(index => index < row.Length ? row[index] : null).ToArray();
                        group = new Group(values, measures.Select(measure => new DataAccumulator(measure.Function)).ToArray());
                        groups[key] = group;
                        order.Add(group);
                        size += GroupOverhead + (2L * key.Length) + DataRowSize.Estimate(values) + (MeasureOverhead * measures.Count);
                    }

                    for (var index = 0; index < measures.Count; index++)
                    {
                        var fieldIndex = measures[index].FieldIndex;
                        var value = fieldIndex < 0 ? true : fieldIndex < row.Length ? row[fieldIndex] : null;

                        size += group.Accumulators[index].Add(value);
                    }

                    if (remaining is null && size > options.MemoryLimit)
                    {
                        remaining = new DataRowBuffer(options);
                    }
                }

                // Without group fields, an empty input still has one summary row, like a SQL aggregate.
                if (!any && groupIndexes.Count == 0)
                {
                    order.Add(new Group([], measures.Select(measure => new DataAccumulator(measure.Function)).ToArray()));
                }

                foreach (var group in order)
                {
                    yield return [.. group.Values, .. group.Accumulators.Select(accumulator => accumulator.Result())];
                }

                previous?.Dispose();
                previous = remaining;

                if (remaining is null)
                {
                    yield break;
                }

                source = DataAsyncRows.From(remaining.Read(), cancellationToken);
            }
        }
        finally
        {
            previous?.Dispose();
        }
    }

    private sealed record Group(object[] Values, DataAccumulator[] Accumulators);
}
