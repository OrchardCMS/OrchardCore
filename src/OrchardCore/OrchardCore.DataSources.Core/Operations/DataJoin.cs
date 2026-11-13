using System.Runtime.CompilerServices;

namespace OrchardCore.DataSources.Operations;

/// <summary>
/// Joins two sets of rows on the values of key fields, like a SQL join, with a bounded amount of memory.
/// </summary>
public static class DataJoin
{
    /// <summary>
    /// Joins the rows of two inputs. The right input is read first. When it fits in
    /// <see cref="DataSpillOptions.MemoryLimit"/>, it becomes a lookup the left rows stream through; otherwise both
    /// inputs are sorted by key in temporary files and merged. Either way, the rows come out in the same order: each
    /// left row with its matches in the order of the right input, then the right rows without a match. Rows whose key
    /// has a missing value never match.
    /// </summary>
    /// <param name="left">The rows of the left input.</param>
    /// <param name="right">The rows of the right input.</param>
    /// <param name="leftWidth">The number of fields of the left rows.</param>
    /// <param name="rightWidth">The number of fields of the right rows.</param>
    /// <param name="leftKeys">The positions of the key fields of the left rows.</param>
    /// <param name="rightKeys">The positions of the key fields of the right rows, matched with
    /// <paramref name="leftKeys"/> in order.</param>
    /// <param name="joinType">Which rows without a match are kept.</param>
    /// <param name="ignoreCase">Whether text keys match ignoring case.</param>
    /// <param name="options">The memory limit and the directory of the temporary files.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The joined rows: the <paramref name="leftWidth"/> fields of the left row, then the
    /// <paramref name="rightWidth"/> fields of the right row; the fields of a side without a match are empty.</returns>
    public static async IAsyncEnumerable<object[]> JoinAsync(
        IAsyncEnumerable<object[]> left,
        IAsyncEnumerable<object[]> right,
        int leftWidth,
        int rightWidth,
        IReadOnlyList<int> leftKeys,
        IReadOnlyList<int> rightKeys,
        DataJoinType joinType,
        bool ignoreCase,
        DataSpillOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        ArgumentNullException.ThrowIfNull(leftKeys);
        ArgumentNullException.ThrowIfNull(rightKeys);
        ArgumentNullException.ThrowIfNull(options);

        if (leftKeys.Count != rightKeys.Count)
        {
            throw new ArgumentException("A join needs as many left keys as right keys.", nameof(rightKeys));
        }

        var shape = new Shape(leftWidth, rightWidth, leftKeys, rightKeys, ignoreCase, joinType is DataJoinType.Left or DataJoinType.Full, joinType is DataJoinType.Right or DataJoinType.Full);

        using var rightRows = new DataRowBuffer(options);

        await foreach (var row in right.WithCancellation(cancellationToken))
        {
            rightRows.Add(row);
        }

        var joined = rightRows.InMemoryRows is { } inMemory
            ? HashJoinAsync(left, inMemory, shape, cancellationToken)
            : MergeJoinAsync(left, rightRows, shape, options, cancellationToken);

        await foreach (var row in joined.WithCancellation(cancellationToken))
        {
            yield return row;
        }
    }

    private static async IAsyncEnumerable<object[]> HashJoinAsync(
        IAsyncEnumerable<object[]> left,
        IReadOnlyList<object[]> right,
        Shape shape,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var lookup = new Dictionary<string, List<int>>(StringComparer.Ordinal);

        for (var index = 0; index < right.Count; index++)
        {
            if (DataKeys.HasEmptyKey(right[index], shape.RightKeys))
            {
                continue;
            }

            var key = DataKeys.Build(right[index], shape.RightKeys, shape.IgnoreCase);

            if (!lookup.TryGetValue(key, out var list))
            {
                list = [];
                lookup[key] = list;
            }

            list.Add(index);
        }

        var matched = shape.KeepRight ? new bool[right.Count] : null;

        await foreach (var row in left.WithCancellation(cancellationToken))
        {
            var found = false;

            if (!DataKeys.HasEmptyKey(row, shape.LeftKeys) &&
                lookup.TryGetValue(DataKeys.Build(row, shape.LeftKeys, shape.IgnoreCase), out var indexes))
            {
                foreach (var index in indexes)
                {
                    if (matched is not null)
                    {
                        matched[index] = true;
                    }

                    found = true;

                    yield return shape.Combine(row, right[index]);
                }
            }

            if (!found && shape.KeepLeft)
            {
                yield return shape.Combine(row, null);
            }
        }

        if (matched is null)
        {
            yield break;
        }

        for (var index = 0; index < right.Count; index++)
        {
            if (!matched[index])
            {
                yield return shape.Combine(null, right[index]);
            }
        }
    }

    private static async IAsyncEnumerable<object[]> MergeJoinAsync(
        IAsyncEnumerable<object[]> left,
        DataRowBuffer right,
        Shape shape,
        DataSpillOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Tag each row with its key and position, sort both inputs by key, merge them, and sort the joined rows back to
        // the order of the lookup join: by left position, then right position, the right rows without a match last.
        var byKey = Comparer<object[]>.Create((x, y) =>
        {
            var result = string.CompareOrdinal((string)x[0], (string)y[0]);

            return result != 0 ? result : ((long)x[1]).CompareTo((long)y[1]);
        });

        var byPosition = Comparer<object[]>.Create((x, y) =>
        {
            var result = ((long)x[0]).CompareTo((long)y[0]);

            return result != 0 ? result : ((long)x[1]).CompareTo((long)y[1]);
        });

        var sortedRight = DataSorter.SortAsync(Tag(DataAsyncRows.From(right.Read(), cancellationToken), shape.RightKeys, shape.IgnoreCase, cancellationToken), byKey, options, cancellationToken);
        var sortedLeft = DataSorter.SortAsync(Tag(left, shape.LeftKeys, shape.IgnoreCase, cancellationToken), byKey, options, cancellationToken);
        var merged = MergeAsync(sortedLeft, sortedRight, shape, options, cancellationToken);

        await foreach (var row in DataSorter.SortAsync(merged, byPosition, options, cancellationToken))
        {
            yield return row[2..];
        }
    }

    private static async IAsyncEnumerable<object[]> MergeAsync(
        IAsyncEnumerable<object[]> sortedLeft,
        IAsyncEnumerable<object[]> sortedRight,
        Shape shape,
        DataSpillOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var leftRows = sortedLeft.GetAsyncEnumerator(cancellationToken);
        await using var rightRows = sortedRight.GetAsyncEnumerator(cancellationToken);

        var hasLeft = await leftRows.MoveNextAsync();
        var hasRight = await rightRows.MoveNextAsync();

        while (hasLeft || hasRight)
        {
            var leftKey = hasLeft ? (string)leftRows.Current[0] : null;
            var rightKey = hasRight ? (string)rightRows.Current[0] : null;

            // A missing key never matches.
            if (hasLeft && leftKey is null)
            {
                if (shape.KeepLeft)
                {
                    yield return Untag(leftRows.Current, null, shape);
                }

                hasLeft = await leftRows.MoveNextAsync();

                continue;
            }

            if (hasRight && rightKey is null)
            {
                if (shape.KeepRight)
                {
                    yield return Untag(null, rightRows.Current, shape);
                }

                hasRight = await rightRows.MoveNextAsync();

                continue;
            }

            var comparison = !hasLeft ? 1 : !hasRight ? -1 : string.CompareOrdinal(leftKey, rightKey);

            if (comparison < 0)
            {
                if (shape.KeepLeft)
                {
                    yield return Untag(leftRows.Current, null, shape);
                }

                hasLeft = await leftRows.MoveNextAsync();
            }
            else if (comparison > 0)
            {
                if (shape.KeepRight)
                {
                    yield return Untag(null, rightRows.Current, shape);
                }

                hasRight = await rightRows.MoveNextAsync();
            }
            else
            {
                // The right rows of the key, which a key repeated many times may spill to a temporary file.
                using var group = new DataRowBuffer(options);

                while (hasRight && string.Equals((string)rightRows.Current[0], leftKey, StringComparison.Ordinal))
                {
                    group.Add(rightRows.Current);
                    hasRight = await rightRows.MoveNextAsync();
                }

                while (hasLeft && string.Equals((string)leftRows.Current[0], leftKey, StringComparison.Ordinal))
                {
                    foreach (var match in group.Read())
                    {
                        yield return Untag(leftRows.Current, match, shape);
                    }

                    hasLeft = await leftRows.MoveNextAsync();
                }
            }
        }
    }

    private static async IAsyncEnumerable<object[]> Tag(IAsyncEnumerable<object[]> rows, IReadOnlyList<int> keys, bool ignoreCase, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var position = 0L;

        await foreach (var row in rows.WithCancellation(cancellationToken))
        {
            var key = DataKeys.HasEmptyKey(row, keys) ? null : DataKeys.Build(row, keys, ignoreCase);

            yield return [key, position++, .. row];
        }
    }

    // Builds a joined row tagged with the positions of its left and right rows.
    private static object[] Untag(object[] taggedLeft, object[] taggedRight, Shape shape)
    {
        var combined = shape.Combine(taggedLeft?[2..], taggedRight?[2..]);
        var row = new object[combined.Length + 2];

        row[0] = taggedLeft is null ? long.MaxValue : (long)taggedLeft[1];
        row[1] = taggedRight is null ? -1L : (long)taggedRight[1];
        Array.Copy(combined, 0, row, 2, combined.Length);

        return row;
    }

    private sealed record Shape(int LeftWidth, int RightWidth, IReadOnlyList<int> LeftKeys, IReadOnlyList<int> RightKeys, bool IgnoreCase, bool KeepLeft, bool KeepRight)
    {
        public object[] Combine(object[] left, object[] right)
        {
            var row = new object[LeftWidth + RightWidth];

            if (left is not null)
            {
                Array.Copy(left, row, Math.Min(left.Length, LeftWidth));
            }

            if (right is not null)
            {
                Array.Copy(right, 0, row, LeftWidth, Math.Min(right.Length, RightWidth));
            }

            return row;
        }
    }
}
