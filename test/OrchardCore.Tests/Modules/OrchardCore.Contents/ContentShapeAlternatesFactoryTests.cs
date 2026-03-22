using OrchardCore.Contents;

namespace OrchardCore.Tests.Modules.OrchardCore.Contents;

public class ContentShapeAlternatesFactoryTests
{
    private const int CacheCapacity = 1_000;

    public ContentShapeAlternatesFactoryTests()
    {
        ContentShapeAlternatesFactory.ClearCache();
    }

    [Fact]
    public void GetAlternates_ShouldReturnCachedInstance_ForSameKey()
    {
        var alternates = ContentShapeAlternatesFactory.GetAlternates("BlogPost", "1", "Summary");

        var cachedAlternates = ContentShapeAlternatesFactory.GetAlternates("BlogPost", "1", "Summary");

        Assert.Same(alternates, cachedAlternates);
    }

    [Fact]
    public void GetAlternates_ShouldEvictLeastRecentlyUsedEntry_WhenCapacityIsExceeded()
    {
        var evictedAlternates = ContentShapeAlternatesFactory.GetAlternates("Article", "evict-me", "Detail");

        for (var i = 0; i < CacheCapacity + 5; i++)
        {
            ContentShapeAlternatesFactory.GetAlternates("Article", $"item-{i}", "Detail");
        }

        var alternatesAfterEviction = ContentShapeAlternatesFactory.GetAlternates("Article", "evict-me", "Detail");

        Assert.NotSame(evictedAlternates, alternatesAfterEviction);
    }

    [Fact]
    public void GetAlternates_ShouldKeepMostRecentlyUsedEntry_WhenCapacityIsExceeded()
    {
        var leastRecentlyUsedAlternates = ContentShapeAlternatesFactory.GetAlternates("Page", "cold", "Summary");
        var mostRecentlyUsedAlternates = ContentShapeAlternatesFactory.GetAlternates("Page", "hot", "Summary");

        for (var i = 0; i < CacheCapacity - 2; i++)
        {
            ContentShapeAlternatesFactory.GetAlternates("Page", $"seed-{i}", "Summary");
        }

        ContentShapeAlternatesFactory.GetAlternates("Page", "hot", "Summary");

        for (var i = 0; i < 10; i++)
        {
            ContentShapeAlternatesFactory.GetAlternates("Page", $"overflow-{i}", "Summary");
        }

        var leastRecentlyUsedAlternatesAfterTrim = ContentShapeAlternatesFactory.GetAlternates("Page", "cold", "Summary");
        var mostRecentlyUsedAlternatesAfterTrim = ContentShapeAlternatesFactory.GetAlternates("Page", "hot", "Summary");

        Assert.NotSame(leastRecentlyUsedAlternates, leastRecentlyUsedAlternatesAfterTrim);
        Assert.Same(mostRecentlyUsedAlternates, mostRecentlyUsedAlternatesAfterTrim);
    }
}
