namespace OrchardCore.DisplayManagement.Zones;

/// <summary>
/// Compares dot/colon separated position strings.
/// <para>
/// The total order, from first to last, is:
/// <c>start</c> &lt; <c>before</c> &lt; numbers &lt; <c>after</c> &lt; free-text &lt; <c>end</c>.
/// </para>
/// <para>
/// <c>before</c> and <c>after</c> are anchors within the numeric range (they normalize to very low
/// and very high numbers), so a free-text position such as one produced by <c>PrefixPosition()</c>
/// still sorts after <c>after</c>. Use the terminal sentinels <c>start</c> and <c>end</c> when an
/// item must sort truly first or truly last regardless of any other position, including free text.
/// Both sentinels support sub-ordering, e.g. <c>end.50</c> sorts before <c>end.100</c>.
/// </para>
/// </summary>
public sealed class FlatPositionComparer : IComparer<IPositioned>, IComparer<string>
{
    private static readonly char[] s_splitChars = ['.', ':'];

    public static FlatPositionComparer Instance { get; private set; }

    static FlatPositionComparer()
    {
        Instance = new FlatPositionComparer();
    }

    private FlatPositionComparer()
    {
    }

    public int Compare(IPositioned a, IPositioned b)
    {
        if (ReferenceEquals(a, b))
        {
            return 0;
        }

        if (a is null)
        {
            return -1;
        }
        else if (b is null)
        {
            return 1;
        }

        var x = a.Position;
        var y = b.Position;

        return Compare(x, y);
    }

    public int Compare(string x, string y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        // Handle null/whitespace normalization without string allocation
        var xSpan = GetNormalizedSpan(x);
        var ySpan = GetNormalizedSpan(y);

#if NET9_0_OR_GREATER
        var xParts = xSpan.SplitAny(s_splitChars);
        var yParts = ySpan.SplitAny(s_splitChars);
#else
        var xParts = new SpanSplitEnumerator(xSpan, _splitChars);
        var yParts = new SpanSplitEnumerator(ySpan, _splitChars);
#endif

        var partIndex = 0;

        while (xParts.MoveNext())
        {
            if (!yParts.MoveNext())
            {
                // x is further defined meaning it comes after y
                return 1;
            }

#if NET9_0_OR_GREATER
            var xPart = xSpan[xParts.Current];
            var yPart = ySpan[yParts.Current];
#else
            var xPart = xParts.Current;
            var yPart = yParts.Current;
#endif

            // Terminal sentinels: `start` always sorts before everything else and `end` always
            // sorts after everything else, regardless of numbers, `before`/`after`, or free text.
            var xTier = GetSentinelTier(xPart);
            var yTier = GetSentinelTier(yPart);

            if (xTier != yTier)
            {
                return xTier < yTier ? -1 : 1;
            }

            if (xTier != 0)
            {
                // Both partitions are the same terminal sentinel (e.g. `end` vs `end.100`); the
                // ordering is decided by the remaining partitions.
                partIndex++;
                continue;
            }

            // Normalize known partitions
            var xIsInt = TryNormalizeKnownPartitions(xPart, out var xPos);
            var yIsInt = TryNormalizeKnownPartitions(yPart, out var yPos);

            if (!xIsInt)
            {
                xIsInt = xPart.IsEmpty || int.TryParse(xPart, out xPos);
            }

            if (!yIsInt)
            {
                yIsInt = yPart.IsEmpty || int.TryParse(yPart, out yPos);
            }

            if (!xIsInt && !yIsInt)
            {
                // Fall back to string comparison of original spans
                return xSpan.CompareTo(ySpan, StringComparison.OrdinalIgnoreCase);
            }

            // Non-int after int or greater x pos than y pos (which is an int)
            if (!xIsInt || (yIsInt && xPos > yPos))
            {
                return 1;
            }

            if (!yIsInt || xPos < yPos)
            {
                return -1;
            }

            partIndex++;
        }

        // Check if y has more parts
        if (yParts.MoveNext())
        {
            return -1;
        }

        return 0;
    }

    private static ReadOnlySpan<char> GetNormalizedSpan(string value)
    {
        if (value is null)
        {
            return "before.";
        }

        var span = value.AsSpan();
        if (span.IsWhiteSpace())
        {
            return "0";
        }

        // Trim ':' from start and '.' from end
        span = span.Trim(':');
        span = span.TrimEnd('.');

        return span;
    }

    private static int GetSentinelTier(ReadOnlySpan<char> partition)
    {
        // `start` sorts before every non-sentinel position, `end` sorts after every non-sentinel
        // position (including free text). Everything else lives in the middle tier.
        if (partition.Equals("start", StringComparison.OrdinalIgnoreCase))
        {
            return -1;
        }

        if (partition.Equals("end", StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        return 0;
    }

    private static bool TryNormalizeKnownPartitions(ReadOnlySpan<char> partition, out int position)
    {
        if (partition.Length < 5) // known partitions are long
        {
            position = 0;
            return false;
        }

        if (partition.Equals("before", StringComparison.OrdinalIgnoreCase))
        {
            position = -9999;
            return true;
        }

        if (partition.Equals("after", StringComparison.OrdinalIgnoreCase))
        {
            position = 9999;
            return true;
        }

        position = 0;
        return false;
    }

#if !NET9_0_OR_GREATER
    private ref struct SpanSplitEnumerator
    {
        private readonly ReadOnlySpan<char> _span;
        private readonly ReadOnlySpan<char> _separators;
        private int _currentIndex;

        public SpanSplitEnumerator(ReadOnlySpan<char> span, ReadOnlySpan<char> separators)
        {
            _span = span;
            _separators = separators;
            _currentIndex = 0;
            Current = default;
        }

        public ReadOnlySpan<char> Current { get; private set; }

        public bool MoveNext()
        {
            if (_currentIndex > _span.Length)
            {
                return false;
            }

            if (_currentIndex == _span.Length)
            {
                Current = ReadOnlySpan<char>.Empty;
                _currentIndex = _span.Length + 1; // Set past end to stop iteration
                return true;
            }

            var remaining = _span[_currentIndex..];
            var nextSeparator = remaining.IndexOfAny(_separators);

            if (nextSeparator == -1)
            {
                // No more separators, return the rest of the span
                Current = remaining;
                _currentIndex = _span.Length + 1; // Set past end to stop iteration
            }
            else
            {
                // Return the part before the separator (could be empty)
                Current = remaining[..nextSeparator];
                _currentIndex += nextSeparator + 1; // Move past the separator
            }

            return true;
        }
    }
#endif
}
