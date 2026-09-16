using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.HomeRoute.Endpoints;
using OrchardCore.HomeRoute.Services;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Modules.OrchardCore.HomeRoute;

public class HomeRouteManagementEndpointsTests
{
    [Fact]
    public async Task SetHomeContentAsync_PublishedContent_SetsContentDisplayRoute()
    {
        var contentItem = new ContentItem
        {
            ContentItemId = "home-id",
            ContentType = "LandingPage",
            DisplayText = "Home",
            Published = true,
        };
        var contentManager = new Mock<IContentManager>();
        contentManager
            .Setup(manager => manager.GetAsync("home-id", VersionOptions.Published))
            .ReturnsAsync(contentItem);
        var site = new Mock<ISite>();
        site.SetupProperty(settings => settings.HomeRoute, []);
        var siteService = new Mock<ISiteService>();
        siteService.Setup(service => service.LoadSiteSettingsAsync()).ReturnsAsync(site.Object);

        var result = await HomeRouteManagementEndpoints.SetHomeContentAsync(
            new DefaultHttpContext(),
            "home-id",
            CreateAuthorizationService().Object,
            contentManager.Object,
            new HomeRouteService(siteService.Object));

        Assert.Equal(StatusCodes.Status200OK, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        Assert.Equal("OrchardCore.Contents", site.Object.HomeRoute["Area"]);
        Assert.Equal("Item", site.Object.HomeRoute["Controller"]);
        Assert.Equal("Display", site.Object.HomeRoute["Action"]);
        Assert.Equal("home-id", site.Object.HomeRoute["ContentItemId"]);
        siteService.Verify(service => service.UpdateSiteSettingsAsync(site.Object), Times.Once);
    }

    [Fact]
    public async Task SetHomeContentAsync_CurrentContentRoute_DoesNotWriteAgain()
    {
        var contentItem = new ContentItem { ContentItemId = "home-id", Published = true };
        var contentManager = Mock.Of<IContentManager>(manager =>
            manager.GetAsync("home-id", VersionOptions.Published) == Task.FromResult(contentItem));
        var homeRoute = new RouteValueDictionary
        {
            ["Area"] = "OrchardCore.Contents",
            ["Controller"] = "Item",
            ["Action"] = "Display",
            ["ContentItemId"] = "home-id",
        };
        var site = Mock.Of<ISite>(settings => settings.HomeRoute == homeRoute);
        var siteService = new Mock<ISiteService>();
        siteService.Setup(service => service.LoadSiteSettingsAsync()).ReturnsAsync(site);

        var result = await HomeRouteManagementEndpoints.SetHomeContentAsync(
            new DefaultHttpContext(),
            "home-id",
            CreateAuthorizationService().Object,
            contentManager,
            new HomeRouteService(siteService.Object));

        Assert.Equal(StatusCodes.Status200OK, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        siteService.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
    }

    [Theory]
    [InlineData("JsonPath", "$.BagPart.ContentItems[0]")]
    [InlineData("Legacy", "value")]
    public async Task SetHomeContentAsync_CurrentContainerWithExtraRouteValues_ClearsExtraValues(string key, string value)
    {
        var contentItem = new ContentItem { ContentItemId = "home-id", Published = true };
        var contentManager = Mock.Of<IContentManager>(manager =>
            manager.GetAsync("home-id", VersionOptions.Published) == Task.FromResult(contentItem));
        var homeRoute = new RouteValueDictionary
        {
            ["Area"] = "OrchardCore.Contents",
            ["Controller"] = "Item",
            ["Action"] = "Display",
            ["ContentItemId"] = "home-id",
        };
        homeRoute[key] = value;
        var siteMock = new Mock<ISite>();
        siteMock.SetupProperty(settings => settings.HomeRoute, homeRoute);
        var site = siteMock.Object;
        var siteService = new Mock<ISiteService>();
        siteService.Setup(service => service.LoadSiteSettingsAsync()).ReturnsAsync(site);

        var result = await HomeRouteManagementEndpoints.SetHomeContentAsync(
            new DefaultHttpContext(),
            "home-id",
            CreateAuthorizationService().Object,
            contentManager,
            new HomeRouteService(siteService.Object));

        Assert.Equal(StatusCodes.Status200OK, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        Assert.Equal(4, site.HomeRoute.Count);
        Assert.False(site.HomeRoute.ContainsKey(key));
        siteService.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Once);
    }

    [Fact]
    public async Task SetHomeContentAsync_DeniedPermission_DoesNotAccessContentOrSettings()
    {
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(),
            It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Failed());
        var content = new Mock<IContentManager>(MockBehavior.Strict);
        var route = new Mock<IHomeRouteService>(MockBehavior.Strict);

        var result = await HomeRouteManagementEndpoints.SetHomeContentAsync(new DefaultHttpContext { RequestServices = services }, "home-id",
            authorization.Object, content.Object, route.Object);

        Assert.Equal(403, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        content.VerifyNoOtherCalls();
        route.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SetHomeContentAsync_NoPublishedVersion_DoesNotWriteSettings()
    {
        var content = new Mock<IContentManager>(MockBehavior.Strict);
        content.Setup(service => service.GetAsync("home-id", VersionOptions.Published)).ReturnsAsync((ContentItem)null);
        var route = new Mock<IHomeRouteService>(MockBehavior.Strict);

        var result = await HomeRouteManagementEndpoints.SetHomeContentAsync(new DefaultHttpContext(), "home-id",
            CreateAuthorizationService().Object, content.Object, route.Object);

        Assert.Equal(404, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        route.VerifyNoOtherCalls();
    }

    private static Mock<IAuthorizationService> CreateAuthorizationService()
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        return authorizationService;
    }
}
