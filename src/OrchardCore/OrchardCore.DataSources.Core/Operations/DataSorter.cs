using System.Runtime.CompilerServices;

namespace OrchardCore.DataSources.Operations;

/// <summary>
/// Sorts any number of rows with a bounded amount of memory: rows are sorted in memory up to
/// <see cref="DataSpillOptions.MemoryLimit"/>, written to temporary files as sorted runs when there are more, and the
/// runs are merged while the rows are read. The sort is stable: rows that compare equal keep their order.
/// </summary>
public static class DataSorter
{
    // The most runs merged at once, which bounds the number of open files.
    private const int MaxRunsPerMerge = 64;

    /// <summary>
    /// Creates a comparer of rows by fields, under the rules of <see cref="DataValues.Compare(object, object)"/>:
    /// numbers by value, text ignoring case, and missing values first.
    /// </summary>
    /// <param name="keys">The positions of the fields to sort by, the most significant first, and whether the
    /// largest values come first.</param>
    /// <returns>The comparer.</returns>
    public static IComparer<object[]> CreateComparer(IReadOnlyList<(int Index, bool Descending)> keys)
    {
        ArgumentNullException.ThrowIfNull(keys);

        var copy = keys.ToArray();

        return Comparer<object[]>.Create((left, right) =>
        {
            foreach (var (index, descending) in copy)
            {
                var result = DataValues.Compare(Value(left, index), Value(right, index));

                if (result != 0)
                {
                    return descending ? -result : result;
                }
            }

            return 0;
        });
    }

    /// <summary>
    /// Sorts rows.
    /// </summary>
    /// <param name="rows">The rows to sort.</param>
    /// <param name="comparer">The order of the rows.</param>
    /// <param name="options">The memory limit and the directory of the temporary files.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The sorted rows, available once every row was read.</returns>
    public static async IAsyncEnumerable<object[]> SortAsync(
        IAsyncEnumerable<object[]> rows,
        IComparer<object[]> comparer,
        DataSpillOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(comparer);
        ArgumentNullException.ThrowIfNull(options);

        var runs = new List<DataRowFile>();

        try
        {
            var chunk = new List<object[]>();
            var size = 0L;

            await foreach (var row in rows.WithCancellation(cancellationToken))
            {
                chunk.Add(row);
                size += DataRowSize.Estimate(row);

                if (size > options.MemoryLimit)
                {
                    runs.Add(WriteRun(Sort(chunk, comparer), options));
                    chunk = [];
                    size = 0;
                }
            }

            var last = Sort(chunk, comparer);

            if (runs.Count == 0)
            {
                foreach (var row in last)
                {
                    yield return row;
                }

                yield break;
            }

            // Merge consecutive runs together, level by level, until they can all be merged at once. Merging consecutive
            // runs keeps equal rows in their order.
            while (runs.Count + 1 > MaxRunsPerMerge)
            {
                var level = new List<DataRowFile>();

                for (var start = 0; start < runs.Count; start += MaxRunsPerMerge)
                {
                    var group = runs.GetRange(start, Math.Min(MaxRunsPerMerge, runs.Count - start));

                    if (group.Count == 1)
                    {
                        level.Add(group[0]);

                        continue;
                    }

                    var merged = DataRowFile.Create(options.TemporaryDirectory);

                    foreach (var row in Merge(group.Select(run => run.Read()).ToList(), comparer, cancellationToken))
                    {
                        merged.Write(row);
                    }

                    foreach (var run in group)
                    {
                        run.Dispose();
                    }

                    level.Add(merged);
                }

                runs = level;
            }

            var sources = runs.Select(run => run.Read()).Append(last).ToList();

            foreach (var row in Merge(sources, comparer, cancellationToken))
            {
                yield return row;
            }
        }
        finally
        {
            foreach (var run in runs)
            {
                run.Dispose();
            }
        }
    }

    private static List<object[]> Sort(List<object[]> rows, IComparer<object[]> comparer)
    {
        return rows.Count < 2 ? rows : rows.Order(comparer).ToList();
    }

    private static DataRowFile WriteRun(List<object[]> rows, DataSpillOptions options)
    {
        var run = DataRowFile.Create(options.TemporaryDirectory);

        foreach (var row in rows)
        {
            run.Write(row);
        }

        return run;
    }

    private static IEnumerable<object[]> Merge(List<IEnumerable<object[]>> sources, IComparer<object[]> comparer, CancellationToken cancellationToken)
    {
        var enumerators = new List<IEnumerator<object[]>>(sources.Count);

        // Equal rows come from the earliest source first, which keeps the sort stable.
        var queue = new PriorityQueue<int, (object[] Row, int Source)>(Comparer<(object[] Row, int Source)>.Create((left, right) =>
        {
            var result = comparer.Compare(left.Row, right.Row);

            return result != 0 ? result : left.Source.CompareTo(right.Source);
        }));

        try
        {
            for (var index = 0; index < sources.Count; index++)
            {
                var enumerator = sources[index].GetEnumerator();
                enumerators.Add(enumerator);

                if (enumerator.MoveNext())
                {
                    queue.Enqueue(index, (enumerator.Current, index));
                }
            }

            var count = 0;

            while (queue.TryDequeue(out var source, out var item))
            {
                if (++count % 4096 == 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                yield return item.Row;

                var enumerator = enumerators[source];

                if (enumerator.MoveNext())
                {
                    queue.Enqueue(source, (enumerator.Current, source));
                }
            }
        }
        finally
        {
            foreach (var enumerator in enumerators)
            {
                enumerator.Dispose();
            }
        }
    }

    private static object Value(object[] row, int index)
    {
        return index >= 0 && index < row.Length ? row[index] : null;
    }
}
