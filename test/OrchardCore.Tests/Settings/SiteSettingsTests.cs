using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Settings;

public class SiteSettingsTests
{
    [Fact]
    public void TryGetReturnsCachedSettingsWithoutAllocatingNewInstances_Default_Succeeds()
    {
        var site = new SiteSettings();
        site.Put(new TestSettings { Name = "alpha" });

        Assert.True(site.TryGet<TestSettings>(out var first));
        Assert.True(site.TryGet<TestSettings>(out var second));

        Assert.Same(first, second);
        Assert.Equal("alpha", second.Name);
    }

    [Fact]
    public void TryGetReturnsNull_SettingsAreMissing_Succeeds()
    {
        var site = new SiteSettings();

        Assert.False(site.TryGet<TestSettings>(out var settings));
        Assert.Null(settings);
    }

    [Fact]
    public void RemoveDeletesStoredProperties_Default_Succeeds()
    {
        var site = new SiteSettings();
        site.Put(new TestSettings { Name = "alpha" });

        Assert.True(site.Remove<TestSettings>());
        Assert.False(site.TryGet<TestSettings>(out var settings));
        Assert.Null(settings);
    }

    private sealed class TestSettings
    {
        public string Name { get; set; }
    }
}
