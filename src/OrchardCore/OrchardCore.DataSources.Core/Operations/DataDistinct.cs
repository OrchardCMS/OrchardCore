using System.Runtime.CompilerServices;

namespace OrchardCore.DataSources.Operations;

/// <summary>
/// Removes the rows whose key fields hold the same values as a row before them, with a bounded amount of memory, and
/// keeps the order of the rows.
/// </summary>
public static class DataDistinct
{
    // The memory a key takes in the set of keys, besides its characters.
    private const int KeyOverhead = 64;

    /// <summary>
    /// Keeps the first row of each set of rows with equal keys. The keys stay in memory up to
    /// <see cref="DataSpillOptions.MemoryLimit"/>; the rows of the keys that don't fit are written to a temporary file
    /// and deduplicated once the others are done, which keeps the order of the rows.
    /// </summary>
    /// <param name="rows">The rows.</param>
    /// <param name="keyIndexes">The positions of the fields that identify a row, under the rules of
    /// <see cref="DataKeys"/>.</param>
    /// <param name="ignoreCase">Whether text keys ignore case.</param>
    /// <param name="options">The memory limit and the directory of the temporary files.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The first row of each key, in the order of the rows, as soon as it is read when the keys fit in memory.</returns>
    public static async IAsyncEnumerable<object[]> DistinctAsync(
        IAsyncEnumerable<object[]> rows,
        IReadOnlyList<int> keyIndexes,
        bool ignoreCase,
        DataSpillOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(keyIndexes);
        ArgumentNullException.ThrowIfNull(options);

        var source = rows;
        DataRowBuffer previous = null;

        try
        {
            // Each pass keeps the keys that fit in memory; the rows of the other keys all come after the last row the
            // pass kept, so the next pass continues the order.
            while (true)
            {
                var seen = new HashSet<string>(StringComparer.Ordinal);
                var size = 0L;
                DataRowBuffer remaining = null;

                await foreach (var row in source.WithCancellation(cancellationToken))
                {
                    var key = DataKeys.Build(row, keyIndexes, ignoreCase);

                    if (seen.Contains(key))
                    {
                        continue;
                    }

                    if (remaining is null)
                    {
                        seen.Add(key);
                        size += KeyOverhead + (2L * key.Length);

                        if (size > options.MemoryLimit)
                        {
                            remaining = new DataRowBuffer(options);
                        }

                        yield return row;
                    }
                    else
                    {
                        remaining.Add(row);
                    }
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
}
