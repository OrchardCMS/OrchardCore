using Microsoft.AspNetCore.Http;
using OrchardCore.Admin;
using OrchardCore.Entities;
using OrchardCore.OpenId.Services;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.OpenId;

public class OpenIdThemeSelectorTests
{
    [Theory]
    [InlineData(false, 100)]
    [InlineData(true, -100)]
    public async Task AccessPagesFollowLoginThemeSetting(bool useSiteTheme, int expectedPriority)
    {
        var site = new SiteSettings();
        site.Put(new LoginSettings { UseSiteTheme = useSiteTheme });
        var siteService = new Mock<ISiteService>();
        siteService.Setup(service => service.GetSiteSettingsAsync()).ReturnsAsync(site);
        var adminThemeService = new Mock<IAdminThemeService>();
        adminThemeService.Setup(service => service.GetAdminThemeNameAsync()).ReturnsAsync("TheAdmin");
        var context = new DefaultHttpContext();
        context.Request.RouteValues["area"] = "OrchardCore.OpenId";
        context.Request.RouteValues["controller"] = "Access";
        var selector = new OpenIdThemeSelector(siteService.Object, adminThemeService.Object,
            new HttpContextAccessor { HttpContext = context });

        var result = await selector.GetThemeAsync();

        Assert.Equal("TheAdmin", result.ThemeName);
        Assert.Equal(expectedPriority, result.Priority);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("OrchardCore.Users", "Access")]
    [InlineData("OrchardCore.OpenId", "Application")]
    public async Task UnrelatedRequestsDoNotSelectLoginTheme(string area, string controller)
    {
        var context = new DefaultHttpContext();
        context.Request.RouteValues["area"] = area;
        context.Request.RouteValues["controller"] = controller;
        var selector = new OpenIdThemeSelector(Mock.Of<ISiteService>(MockBehavior.Strict),
            Mock.Of<IAdminThemeService>(MockBehavior.Strict), new HttpContextAccessor { HttpContext = context });

        Assert.Null(await selector.GetThemeAsync());
    }

    [Fact]
    public async Task MissingHttpContextDoesNotSelectLoginTheme()
    {
        var selector = new OpenIdThemeSelector(Mock.Of<ISiteService>(MockBehavior.Strict),
            Mock.Of<IAdminThemeService>(MockBehavior.Strict), new HttpContextAccessor());

        Assert.Null(await selector.GetThemeAsync());
    }

    [Fact]
    public async Task MissingAdminThemeDoesNotSelectLoginTheme()
    {
        var adminThemeService = new Mock<IAdminThemeService>();
        adminThemeService.Setup(service => service.GetAdminThemeNameAsync()).ReturnsAsync((string)null);
        var context = new DefaultHttpContext();
        context.Request.RouteValues["area"] = "OrchardCore.OpenId";
        context.Request.RouteValues["controller"] = "Access";
        var selector = new OpenIdThemeSelector(Mock.Of<ISiteService>(MockBehavior.Strict),
            adminThemeService.Object, new HttpContextAccessor { HttpContext = context });

        Assert.Null(await selector.GetThemeAsync());
    }
}
