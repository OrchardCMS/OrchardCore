using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Modules.OrchardCore.Settings;

public sealed class PagerOptionsConfigurationTests
{
    [Theory]
    [InlineData(0, 1000, 1000)]
    [InlineData(500, 1000, 500)]
    [InlineData(500, 0, 500)]
    [InlineData(0, 0, 0)]
    public void PostConfigure_MaxPagedCount_SiteValueWinsOnlyWhenSet(int siteValue, int configuredValue, int expected)
    {
        // Arrange
        var siteService = new Mock<ISiteService>();
        siteService.Setup(s => s.GetSiteSettingsAsync()).ReturnsAsync(new SiteSettings { MaxPagedCount = siteValue });

        var options = new PagerOptions { MaxPagedCount = configuredValue };

        // Act
        new PagerOptionsConfiguration(siteService.Object).PostConfigure(null, options);

        // Assert
        Assert.Equal(expected, options.MaxPagedCount);
    }
}
