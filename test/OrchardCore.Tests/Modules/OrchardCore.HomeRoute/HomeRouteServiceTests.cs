using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Autoroute;
using OrchardCore.Autoroute.Handlers;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Environment.Cache;
using OrchardCore.HomeRoute.Services;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Modules.OrchardCore.HomeRoute;

public class HomeRouteServiceTests
{
    [Fact]
    public async Task PublishedAsync_CustomAutorouteKeys_PreservesOtherValuesAndClearsContainedPath()
    {
        var original = new RouteValueDictionary { ["keep"] = "custom", ["nested"] = "BagPart.ContentItems[0]" };
        var site = new Mock<ISite>();
        site.SetupProperty(settings => settings.HomeRoute, original);
        var siteService = new Mock<ISiteService>();
        siteService.Setup(service => service.LoadSiteSettingsAsync()).ReturnsAsync(site.Object);
        var handler = new AutoroutePartHandler(Mock.Of<IAutorouteEntries>(), Options.Create(new AutorouteOptions
        {
            ContentItemIdKey = "document", JsonPathKey = "nested",
            GlobalRouteValues = new RouteValueDictionary { ["Controller"] = "Custom" },
        }), null, null, new HomeRouteService(siteService.Object), Mock.Of<ITagCache>(), null, null,
            Mock.Of<IStringLocalizer<AutoroutePartHandler>>());
        var item = new ContentItem { ContentItemId = "root" };
        item.Alter<AutoroutePart>(part => { part.Path = "root"; part.SetHomepage = true; });
        var part = item.Get<AutoroutePart>(nameof(AutoroutePart));

        await handler.PublishedAsync(new PublishContentContext(item, null), part);

        Assert.Equal("root", site.Object.HomeRoute["document"]);
        Assert.Equal("Custom", site.Object.HomeRoute["Controller"]);
        Assert.Equal("custom", site.Object.HomeRoute["keep"]);
        Assert.False(site.Object.HomeRoute.ContainsKey("nested"));
        Assert.True(original.ContainsKey("nested"));
        Assert.False(part.SetHomepage);
        part.SetHomepage = true;
        await handler.PublishedAsync(new PublishContentContext(item, null), part);
        Assert.False(part.SetHomepage);
        siteService.Verify(service => service.UpdateSiteSettingsAsync(site.Object), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_FailedCallback_DoesNotMutateOriginalRoute()
    {
        var original = new RouteValueDictionary { ["keep"] = "original" };
        var site = Mock.Of<ISite>(settings => settings.HomeRoute == original);
        var siteService = new Mock<ISiteService>();
        siteService.Setup(service => service.LoadSiteSettingsAsync()).ReturnsAsync(site);
        var service = new HomeRouteService(siteService.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(route =>
        {
            route.Clear();
            throw new InvalidOperationException();
        }));

        Assert.Same(original, site.HomeRoute);
        Assert.Equal("original", site.HomeRoute["keep"]);
        siteService.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_EquivalentValues_DoesNotSave()
    {
        var original = new RouteValueDictionary { ["id"] = 42 };
        var site = Mock.Of<ISite>(settings => settings.HomeRoute == original);
        var siteService = new Mock<ISiteService>();
        siteService.Setup(service => service.LoadSiteSettingsAsync()).ReturnsAsync(site);
        var service = new HomeRouteService(siteService.Object);

        Assert.False(await service.UpdateAsync(route => route["ID"] = "42"));
        Assert.Same(original, site.HomeRoute);
        siteService.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_MissingRoute_CanInitializeOrRemainEmpty()
    {
        var site = new Mock<ISite>();
        site.SetupProperty(settings => settings.HomeRoute);
        var siteService = new Mock<ISiteService>();
        siteService.Setup(service => service.LoadSiteSettingsAsync()).ReturnsAsync(site.Object);
        var service = new HomeRouteService(siteService.Object);

        Assert.False(await service.UpdateAsync(route => route.Clear()));
        Assert.True(await service.UpdateAsync(route => route["Controller"] = "Home"));
        Assert.Equal("Home", site.Object.HomeRoute["Controller"]);
        siteService.Verify(service => service.UpdateSiteSettingsAsync(site.Object), Times.Once);
    }
}
