using OrchardCore.DisplayManagement;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Settings;

public class ShapeRenderingOptionsConfigurationTests
{
    [Fact]
    public void ConfigureDisablesShapeDebugInformationByDefault()
    {
        var site = new SiteSettings();
        var siteService = new Mock<ISiteService>();
        siteService.Setup(x => x.GetSiteSettingsAsync())
            .ReturnsAsync(site);

        var sut = new ShapeRenderingOptionsConfiguration(siteService.Object);
        var options = new ShapeRenderingOptions
        {
            WriteShapeDebugInformation = true,
        };

        sut.Configure(options);

        Assert.False(options.WriteShapeDebugInformation);
    }

    [Fact]
    public void ConfigureEnablesShapeDebugInformationWhenConfiguredInSiteSettings()
    {
        var site = new SiteSettings();
        site.Put(new DebugSettings
        {
            WriteShapeDebugInformation = true,
        });

        var siteService = new Mock<ISiteService>();
        siteService.Setup(x => x.GetSiteSettingsAsync())
            .ReturnsAsync(site);

        var sut = new ShapeRenderingOptionsConfiguration(siteService.Object);
        var options = new ShapeRenderingOptions();

        sut.Configure(options);

        Assert.True(options.WriteShapeDebugInformation);
    }
}
