using OrchardCore.Contents;

namespace OrchardCore.Tests.Modules.OrchardCore.Contents;

public class ContentShapeAlternatesFactoryTests
{
    [Fact]
    public void GetEntry_ShouldReturnCachedInstance_ForSameKey()
    {
        var entry = ContentShapeAlternatesFactory.GetEntry("BlogPost", "Summary");
        var cachedEntry = ContentShapeAlternatesFactory.GetEntry("BlogPost", "Summary");

        Assert.Same(entry, cachedEntry);
    }

    [Fact]
    public void GetEntry_ShouldProduceCorrectAlternates_WhenAssembledWithContentItemId()
    {
        var entry = ContentShapeAlternatesFactory.GetEntry("BlogPost", "Summary");

        Assert.Equal(
            ["Content_Summary", "Content__BlogPost", "Content__42", "Content_Summary__BlogPost", "Content_Summary__42"],
            entry.GetAlternates("42"));
    }

    [Fact]
    public void GetEntry_ShouldReturnDifferentInstances_ForDifferentKeys()
    {
        var entryA = ContentShapeAlternatesFactory.GetEntry("BlogPost", "Summary");
        var entryB = ContentShapeAlternatesFactory.GetEntry("Article", "Detail");

        Assert.NotSame(entryA, entryB);
        Assert.NotEqual(entryA.ContentTypeAlternate, entryB.ContentTypeAlternate);
        Assert.NotEqual(entryA.DisplayTypeAlternate, entryB.DisplayTypeAlternate);
    }
}

