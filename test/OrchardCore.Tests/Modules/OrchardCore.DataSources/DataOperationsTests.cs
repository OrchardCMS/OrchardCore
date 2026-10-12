using OrchardCore.DataSources;
using OrchardCore.DataSources.Operations;

namespace OrchardCore.Tests.Modules.OrchardCore.DataSources;

public sealed class DataOperationsTests : IDisposable
{
    // A limit of one byte writes every row to temporary files; the default keeps them in memory.
    public static readonly TheoryData<long> MemoryLimits = new([1L, DataSpillOptions.DefaultMemoryLimit]);

    private readonly string _directory = Path.Combine(Path.GetTempPath(), "oc-data-operations", Guid.NewGuid().ToString("n"));

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public void Read_SpilledBuffer_ReturnsEveryValueWithItsType()
    {
        // Arrange
        var date = new DateTime(2026, 3, 15, 14, 30, 0, DateTimeKind.Utc);
        object[] row = ["text", 42L, 7, 1.5m, 2.25d, true, date, new DateTimeOffset(date, TimeSpan.Zero).ToOffset(TimeSpan.FromHours(2)), Guid.Parse("8d3e5f2a-55a2-4c11-9a5c-6f1f7b0f8a11"), TimeSpan.FromMinutes(3), 1.5f, (short)3, (byte)4, null, new Uri("https://example.com/")];
        using var buffer = new DataRowBuffer(new DataSpillOptions(_directory, memoryLimit: 1));

        // Act
        buffer.Add(row);
        buffer.Add(null);
        var read = buffer.Read().ToList();

        // Assert
        Assert.True(buffer.IsSpilled);
        Assert.Equal(2, buffer.Count);
        Assert.Equal(row[..^1], read[0][..^1]);
        Assert.Equal(DateTimeKind.Utc, ((DateTime)read[0][6]).Kind);
        Assert.Equal("https://example.com/", read[0][^1]);
        Assert.Null(read[1]);
    }

    [Fact]
    public void Read_SpilledBuffer_CanBeReadSeveralTimes()
    {
        // Arrange
        using var buffer = new DataRowBuffer(new DataSpillOptions(_directory, memoryLimit: 100));

        for (var index = 0L; index < 50; index++)
        {
            buffer.Add([index, "row " + index]);
        }

        // Act
        var first = buffer.Read().Select(row => (long)row[0]).ToList();
        var second = buffer.Read().Select(row => (long)row[0]).ToList();

        // Assert
        Assert.True(buffer.IsSpilled);
        Assert.Equal(Enumerable.Range(0, 50).Select(index => (long)index), first);
        Assert.Equal(first, second);
    }

    [Fact]
    public void Dispose_SpilledBuffer_DeletesItsTemporaryFile()
    {
        // Arrange
        var buffer = new DataRowBuffer(new DataSpillOptions(_directory, memoryLimit: 1));
        buffer.Add(["a"]);
        _ = buffer.Read().ToList();

        // Act
        buffer.Dispose();

        // Assert
        Assert.Empty(Directory.GetFiles(_directory));
    }

    [Theory]
    [MemberData(nameof(MemoryLimits))]
    public async Task SortAsync_AnyMemoryLimit_SortsStablyAndDeletesItsFiles(long memoryLimit)
    {
        // Arrange
        var rows = Rows(500, seed: 1);
        var comparer = DataSorter.CreateComparer([(1, false), (2, true)]);

        // Act
        var sorted = await DataSorter.SortAsync(rows.ToAsync(), comparer, Options(memoryLimit), TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        var expected = rows.OrderBy(row => row, comparer).ToList();
        Assert.Equal(expected.Select(row => row[0]), sorted.Select(row => row[0]));
        AssertNoTemporaryFiles();
    }

    [Fact]
    public async Task SortAsync_MoreRunsThanMergedAtOnce_SortsEveryRow()
    {
        // Arrange: about a run per row, more than the 64 merged at once.
        var rows = Rows(300, seed: 2);
        var comparer = DataSorter.CreateComparer([(3, false)]);

        // Act
        var sorted = await DataSorter.SortAsync(rows.ToAsync(), comparer, Options(1), TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(rows.OrderBy(row => row, comparer).Select(row => row[0]), sorted.Select(row => row[0]));
        AssertNoTemporaryFiles();
    }

    [Theory]
    [MemberData(nameof(MemoryLimits))]
    public async Task DistinctAsync_AnyMemoryLimit_KeepsTheFirstRowOfEachKeyInOrder(long memoryLimit)
    {
        // Arrange
        var rows = Rows(400, seed: 3);

        // Act
        var distinct = await DataDistinct.DistinctAsync(rows.ToAsync(), [1, 2], ignoreCase: false, Options(memoryLimit), TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        var expected = rows.DistinctBy(row => (row[1], row[2])).Select(row => row[0]);
        Assert.Equal(expected, distinct.Select(row => row[0]));
        AssertNoTemporaryFiles();
    }

    [Theory]
    [MemberData(nameof(MemoryLimits))]
    public async Task AggregateAsync_AnyMemoryLimit_SummarizesGroupsInOrderOfAppearance(long memoryLimit)
    {
        // Arrange
        var rows = Rows(400, seed: 4);
        DataMeasure[] measures =
        [
            new(DataAggregate.Count, -1),
            new(DataAggregate.Sum, 3),
            new(DataAggregate.Max, 3),
            new(DataAggregate.Median, 3),
            new(DataAggregate.CountDistinct, 2),
        ];

        // Act
        var groups = await DataAggregator.AggregateAsync(rows.ToAsync(), [1], measures, Options(memoryLimit), TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        var expected = rows
            .GroupBy(row => row[1])
            .Select(group => new object[]
            {
                group.Key,
                (long)group.Count(),
                group.Sum(row => (decimal)row[3]),
                group.Max(row => (decimal)row[3]),
                DataAggregations.Median(group.Select(row => row[3]).ToList()),
                (long)group.Select(row => row[2]).Where(code => code is not null).Distinct().Count(),
            })
            .ToList();

        Assert.Equal(expected.Count, groups.Count);

        for (var index = 0; index < expected.Count; index++)
        {
            Assert.Equal(expected[index], groups[index]);
        }

        AssertNoTemporaryFiles();
    }

    [Fact]
    public async Task AggregateAsync_EmptyInputWithoutGroups_ReturnsOneRow()
    {
        // Act
        var groups = await DataAggregator.AggregateAsync(Array.Empty<object[]>().ToAsync(), [], [new(DataAggregate.Count, -1), new(DataAggregate.Sum, 0)], Options(1), TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        var row = Assert.Single(groups);
        Assert.Equal([0L, null], row);
    }

    [Theory]
    [InlineData(DataJoinType.Inner, 1L)]
    [InlineData(DataJoinType.Left, 1L)]
    [InlineData(DataJoinType.Right, 1L)]
    [InlineData(DataJoinType.Full, 1L)]
    [InlineData(DataJoinType.Inner, DataSpillOptions.DefaultMemoryLimit)]
    [InlineData(DataJoinType.Left, DataSpillOptions.DefaultMemoryLimit)]
    [InlineData(DataJoinType.Right, DataSpillOptions.DefaultMemoryLimit)]
    [InlineData(DataJoinType.Full, DataSpillOptions.DefaultMemoryLimit)]
    public async Task JoinAsync_AnyMemoryLimit_ReturnsTheRowsOfALookupJoinInOrder(DataJoinType joinType, long memoryLimit)
    {
        // Arrange: orders with customers, some orders without a customer, some customers without orders, a duplicate
        // customer, and missing keys on both sides.
        object[][] orders =
        [
            ["o1", "c1"],
            ["o2", "c2"],
            ["o3", "c9"],
            ["o4", null],
            ["o5", "C1"],
            ["o6", "c2"],
        ];

        object[][] customers =
        [
            ["c2", "Bea"],
            ["c1", "Ann"],
            ["c3", "Cid"],
            ["c2", "Bea again"],
            [null, "Nobody"],
        ];

        // Act
        var joined = await DataJoin.JoinAsync(orders.ToAsync(), customers.ToAsync(), 2, 2, [1], [0], joinType, ignoreCase: false, Options(memoryLimit), TestContext.Current.CancellationToken)
            .Select(row => $"{row[0] ?? "-"}:{row[3] ?? "-"}")
            .ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        var expected = joinType switch
        {
            DataJoinType.Inner => new[] { "o1:Ann", "o2:Bea", "o2:Bea again", "o6:Bea", "o6:Bea again" },
            DataJoinType.Left => ["o1:Ann", "o2:Bea", "o2:Bea again", "o3:-", "o4:-", "o5:-", "o6:Bea", "o6:Bea again"],
            DataJoinType.Right => ["o1:Ann", "o2:Bea", "o2:Bea again", "o6:Bea", "o6:Bea again", "-:Cid", "-:Nobody"],
            _ => ["o1:Ann", "o2:Bea", "o2:Bea again", "o3:-", "o4:-", "o5:-", "o6:Bea", "o6:Bea again", "-:Cid", "-:Nobody"],
        };

        Assert.Equal(expected, joined);
        AssertNoTemporaryFiles();
    }

    [Theory]
    [MemberData(nameof(MemoryLimits))]
    public async Task JoinAsync_IgnoreCase_MatchesTextKeysIgnoringCase(long memoryLimit)
    {
        // Arrange
        object[][] left = [["a", "X1"], ["b", "x1"]];
        object[][] right = [["x1", 10L]];

        // Act
        var joined = await DataJoin.JoinAsync(left.ToAsync(), right.ToAsync(), 2, 2, [1], [0], DataJoinType.Inner, ignoreCase: true, Options(memoryLimit), TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(["a", "b"], joined.Select(row => row[0]));
    }

    [Fact]
    public async Task JoinAsync_LargeInputs_MatchesTheLookupJoin()
    {
        // Arrange: a limit of a few rows sorts and merges both inputs in temporary files.
        var left = Rows(200, seed: 5);
        var right = Rows(100, seed: 6);

        // Act
        var inMemory = await DataJoin.JoinAsync(left.ToAsync(), right.ToAsync(), 4, 4, [1], [1], DataJoinType.Full, false, Options(DataSpillOptions.DefaultMemoryLimit), TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);
        var spilled = await DataJoin.JoinAsync(left.ToAsync(), right.ToAsync(), 4, 4, [1], [1], DataJoinType.Full, false, Options(4096), TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(inMemory.Count, spilled.Count);
        Assert.Equal(inMemory.Select(row => $"{row[0]}|{row[4]}"), spilled.Select(row => $"{row[0]}|{row[4]}"));
        AssertNoTemporaryFiles();
    }

    private DataSpillOptions Options(long memoryLimit) => new(_directory, memoryLimit);

    private void AssertNoTemporaryFiles()
        => Assert.True(!Directory.Exists(_directory) || Directory.GetFiles(_directory).Length == 0, "The operation left temporary files behind.");

    // Rows of an id, a category among 7, a code among 5, and an amount.
    private static List<object[]> Rows(int count, int seed)
    {
        var random = new Random(seed);

        return Enumerable.Range(0, count)
            .Select(index => new object[] { (long)index, "category " + random.Next(7), random.Next(5) == 0 ? null : "code " + random.Next(5), (decimal)random.Next(1000) / 10 })
            .ToList();
    }
}

internal static class DataOperationsTestExtensions
{
    public static async IAsyncEnumerable<object[]> ToAsync(this IEnumerable<object[]> rows)
    {
        foreach (var row in rows)
        {
            await Task.Yield();

            yield return row;
        }
    }
}
