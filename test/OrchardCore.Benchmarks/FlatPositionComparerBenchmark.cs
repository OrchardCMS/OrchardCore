using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class FlatPositionComparerBenchmark
{
    private readonly FlatPositionComparer _optimizedComparer = FlatPositionComparer.Instance;
    private readonly OriginalFlatPositionComparer _originalComparer = new();
    private readonly string[] _positions;

    public FlatPositionComparerBenchmark()
    {
        // Create a variety of test cases that represent real-world usage
        _positions = 
        [
            "1", "2", "10", "1.1", "1.2", "1.10", "2.5", "before", "after",
            "BEFORE", "AFTER", "1.before", "1.after", "before.1", "after.1",
            "1.2.3", "1.2.10", "test", "alpha", "beta", "1:test", "2:test",
            "test:", "test.", "test..", ":test", "::test", null, "", "   ",
            "0", "00", "01", "1.0", "1.0.0", "complex.position.string",
            "very.long.position.name.with.many.parts", "1.2.3.4.5.6.7.8.9.10",
        ];
    }

    [Benchmark(Baseline = true)]
    public void OriginalCompareStrings()
    {
        for (var i = 0; i < _positions.Length; i++)
        {
            for (var j = 0; j < _positions.Length; j++)
            {
                _ = _originalComparer.Compare(_positions[i], _positions[j]);
            }
        }
    }

    [Benchmark]
    public void OptimizedCompareStrings()
    {
        for (var i = 0; i < _positions.Length; i++)
        {
            for (var j = 0; j < _positions.Length; j++)
            {
                _ = _optimizedComparer.Compare(_positions[i], _positions[j]);
            }
        }
    }

    [Benchmark]
    public void OriginalSortPositions()
    {
        var positions = new string[_positions.Length];
        Array.Copy(_positions, positions, _positions.Length);
        Array.Sort(positions, _originalComparer);
    }

    [Benchmark]
    public void OptimizedSortPositions()
    {
        var positions = new string[_positions.Length];
        Array.Copy(_positions, positions, _positions.Length);
        Array.Sort(positions, _optimizedComparer);
    }

    [Benchmark]
    public void OriginalCompareWithTrimming()
    {
        var positions = new[] { "test:", "test", "test.", "test..", ":test", "::test:" };
        
        for (var i = 0; i < positions.Length; i++)
        {
            for (var j = 0; j < positions.Length; j++)
            {
                _ = _originalComparer.Compare(positions[i], positions[j]);
            }
        }
    }

    [Benchmark]
    public void OptimizedCompareWithTrimming()
    {
        var positions = new[] { "test:", "test", "test.", "test..", ":test", "::test:" };
        
        for (var i = 0; i < positions.Length; i++)
        {
            for (var j = 0; j < positions.Length; j++)
            {
                _ = _optimizedComparer.Compare(positions[i], positions[j]);
            }
        }
    }

    // Original implementation before optimizations
    public class OriginalFlatPositionComparer : IComparer<IPositioned>, IComparer<string>
    {
        public int Compare(IPositioned a, IPositioned b)
        {
            var x = a.Position;
            var y = b.Position;
            return Compare(x, y);
        }

        public int Compare(string x, string y)
        {
            if (x == y)
            {
                return 0;
            }

            // null == "before"; "" == "0"
            x = x == null
                ? "before." // in order to have before < null when 'before' is explicitly defined
                : x.Trim().Length == 0 ? "0" : x.Trim(':').TrimEnd('.'); // ':' is _sometimes_ used as a partition identifier
            y = y == null
                ? "before."
                : y.Trim().Length == 0 ? "0" : y.Trim(':').TrimEnd('.');

            var xParts = x.Split(['.', ':']);
            var yParts = y.Split(['.', ':']);
            for (var i = 0; i < xParts.Length; i++)
            {
                // x is further defined meaning it comes after y (e.g. x == 1.2.3 and y == 1.2)
                if (yParts.Length < i + 1)
                {
                    return 1;
                }

                int xPos, yPos;
                var xPart = string.IsNullOrEmpty(xParts[i]) ? "before" : NormalizeKnownPartitions(xParts[i]);
                var yPart = string.IsNullOrEmpty(yParts[i]) ? "before" : NormalizeKnownPartitions(yParts[i]);

                var xIsInt = int.TryParse(xPart, out xPos);
                var yIsInt = int.TryParse(yPart, out yPos);

                if (!xIsInt && !yIsInt)
                {
                    return string.Compare(string.Join(".", xParts), string.Join(".", yParts), StringComparison.OrdinalIgnoreCase);
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
            }

            // All things being equal y might be further defined than x (e.g. x == 1.2 and y == 1.2.3)
            if (xParts.Length < yParts.Length)
            {
                return -1;
            }

            return 0;
        }

        private static string NormalizeKnownPartitions(string partition)
        {
            if (partition.Length < 5) // known partitions are long
            {
                return partition;
            }

            if (string.Equals(partition, "before", StringComparison.OrdinalIgnoreCase))
            {
                return "-9999";
            }

            if (string.Equals(partition, "after", StringComparison.OrdinalIgnoreCase))
            {
                return "9999";
            }

            return partition;
        }
    }
}
