using OrchardCore.DynamicCache;
using OrchardCore.Environment.Cache;

namespace OrchardCore.Tests.DisplayManagement;

public class CacheContextEntryExtensionsTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GetContextHash_UnsortedEntries_SortsByKeyThenValue(bool useList)
    {
        CacheContextEntry[] entries =
        [
            new("b", "2"),
            new("a", "2"),
            new("a", "1"),
        ];
        IEnumerable<CacheContextEntry> source = useList ? entries.ToList() : entries;

        Assert.Equal("a1a2b2", source.GetContextHash());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GetContextHash_EmptyEntries_ReturnsEmptyString(bool useList)
    {
        IEnumerable<CacheContextEntry> entries = useList ? new List<CacheContextEntry>() : Array.Empty<CacheContextEntry>();

        Assert.Equal(string.Empty, entries.GetContextHash());
    }

    [Fact]
    public void GetContextHash_NullComponents_SkipsNulls()
    {
        CacheContextEntry[] entries =
        [
            new("b", null),
            new(null, "1"),
            new(null, null),
            new("a", "2"),
        ];

        Assert.Equal("1a2b", entries.GetContextHash());
    }

    [Fact]
    public void GetContextHash_Iterator_EnumeratesSourceOnce()
    {
        var enumerationCount = 0;

        IEnumerable<CacheContextEntry> GetEntries()
        {
            enumerationCount++;
            yield return new("a", "1");
            yield return new("b", "2");
        }

        Assert.Equal("a1b2", GetEntries().GetContextHash());
        Assert.Equal(1, enumerationCount);
    }
}
