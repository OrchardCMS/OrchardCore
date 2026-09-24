using OrchardCore.Admin;
using OrchardCore.Admin.Configuration;
using OrchardCore.Admin.Models;
using OrchardCore.Admin.Services;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Entities;
using OrchardCore.Environment.Extensions;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

public class DefaultAdminListLayoutResolverTests
{
    [Fact]
    public async Task GetLayoutAsync_NothingConfigured_ReturnsList()
    {
        var resolver = CreateResolver(new AdminListOptions());

        Assert.Equal(AdminListConstants.List, await resolver.GetLayoutAsync("Contents", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetLayoutAsync_SiteDefault_IsReturned()
    {
        var site = new SiteSettings();
        site.Put(new AdminSettings { ListLayout = AdminListConstants.Table });

        var resolver = CreateResolver(Configure(new AdminListOptions(), site));

        Assert.Equal(AdminListConstants.Table, await resolver.GetLayoutAsync("Contents", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetLayoutAsync_SiteWithoutChoice_UsesTheConfiguredDefault()
    {
        var resolver = CreateResolver(Configure(new AdminListOptions { DefaultLayout = AdminListConstants.Grid }, new SiteSettings()));

        Assert.Equal(AdminListConstants.Grid, await resolver.GetLayoutAsync("Contents", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetLayoutAsync_SiteSetting_WinsOverTheConfiguredDefault()
    {
        var site = new SiteSettings();
        site.Put(new AdminSettings { ListLayout = AdminListConstants.Table });

        var resolver = CreateResolver(Configure(new AdminListOptions { DefaultLayout = AdminListConstants.Grid }, site));

        Assert.Equal(AdminListConstants.Table, await resolver.GetLayoutAsync("Contents", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetLayoutAsync_SiteKeepsTheChoice_IgnoresTheQueryString()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?layout=Grid");

        var resolver = CreateResolver(new AdminListOptions { DefaultLayout = AdminListConstants.Table }, httpContext, AdminListConstants.List, AdminListConstants.Table, AdminListConstants.Grid);

        Assert.Equal(AdminListConstants.Table, await resolver.GetLayoutAsync("Contents", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetLayoutAsync_LayoutInTheQueryString_IsReturnedAndRemembered()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?layout=Grid");

        var options = new AdminListOptions { DefaultLayout = AdminListConstants.Table, AllowUserSelection = true };
        var resolver = CreateResolver(options, httpContext, AdminListConstants.List, AdminListConstants.Table, AdminListConstants.Grid);

        Assert.Equal(AdminListConstants.Grid, await resolver.GetLayoutAsync("Contents", TestContext.Current.CancellationToken));

        // The choice went to the cookie, so the next request opens the list the same way.
        var cookie = httpContext.Response.Headers.SetCookie.ToString();

        Assert.Contains(AdminListLayoutPreference.CookieName, cookie);

        // The value is url encoded on the way out, so the separator reads as %3A here.
        Assert.Contains("Contents%3AGrid", cookie);
    }

    [Fact]
    public async Task GetLayoutAsync_QueryStringInAnotherCase_ReturnsTheDeclaredName()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?layout=grid");

        var options = new AdminListOptions { AllowUserSelection = true };
        var resolver = CreateResolver(options, httpContext, AdminListConstants.List, AdminListConstants.Grid);

        // The name the layout declares is what the alternates, the selector and the cookie carry.
        Assert.Equal(AdminListConstants.Grid, await resolver.GetLayoutAsync("Contents", TestContext.Current.CancellationToken));
        Assert.Contains("Contents%3AGrid", httpContext.Response.Headers.SetCookie.ToString());
    }

    [Fact]
    public async Task GetLayoutAsync_LayoutThisUserPickedBefore_IsReturned()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Cookie = $"{AdminListLayoutPreference.CookieName}=Contents:Grid|Users:Table";

        var options = new AdminListOptions { DefaultLayout = AdminListConstants.List, AllowUserSelection = true };
        var resolver = CreateResolver(options, httpContext, AdminListConstants.List, AdminListConstants.Table, AdminListConstants.Grid);

        Assert.Equal(AdminListConstants.Grid, await resolver.GetLayoutAsync("Contents", TestContext.Current.CancellationToken));
        Assert.Equal(AdminListConstants.Table, await resolver.GetLayoutAsync("Users", TestContext.Current.CancellationToken));

        // A list the user never chose for keeps the default of the site.
        Assert.Equal(AdminListConstants.List, await resolver.GetLayoutAsync("Queries", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetLayoutAsync_LayoutThisSiteCannotRender_IsIgnored()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?layout=Cards");
        httpContext.Request.Headers.Cookie = $"{AdminListLayoutPreference.CookieName}=Users:Cards";

        var options = new AdminListOptions { DefaultLayout = AdminListConstants.List, AllowUserSelection = true };
        var resolver = CreateResolver(options, httpContext, AdminListConstants.List, AdminListConstants.Table, AdminListConstants.Grid);

        Assert.Equal(AdminListConstants.List, await resolver.GetLayoutAsync("Contents", TestContext.Current.CancellationToken));
        Assert.Equal(AdminListConstants.List, await resolver.GetLayoutAsync("Users", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetAvailableLayoutsAsync_ReturnsTheLayoutsOfTheShapeTable()
    {
        var resolver = CreateResolver(new AdminListOptions(), new DefaultHttpContext(), AdminListConstants.Table, AdminListConstants.List);

        Assert.Equal([AdminListConstants.List, AdminListConstants.Table], await resolver.GetAvailableLayoutsAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetAvailableLayoutsAsync_TemplateShapes_ShippedLayoutsAndTheSiteDefaultKeepTheNameTheSettingsStore()
    {
        var withoutDefault = CreateResolver(new AdminListOptions(), new DefaultHttpContext(), "Cards", AdminListConstants.Grid);
        var withDefault = CreateResolver(new AdminListOptions { DefaultLayout = "Cards" }, new DefaultHttpContext(), "Cards", AdminListConstants.Grid);

        // A custom layout keeps the name the shape table gives it, unless the site renders its lists with it.
        Assert.Equal(["cards", AdminListConstants.Grid], await withoutDefault.GetAvailableLayoutsAsync(TestContext.Current.CancellationToken));
        Assert.Equal(["Cards", AdminListConstants.Grid], await withDefault.GetAvailableLayoutsAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetLayoutOptionsAsync_SiteLetsUsersChoose_OffersEveryLayoutKeepingTheRestOfTheQueryString()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/Admin/Contents/ContentItems";
        httpContext.Request.QueryString = new QueryString("?q=post&layout=Table");

        var options = new AdminListOptions { AllowUserSelection = true };
        var resolver = CreateResolver(options, httpContext, AdminListConstants.List, AdminListConstants.Table);

        var layouts = await resolver.GetLayoutOptionsAsync(TestContext.Current.CancellationToken);

        Assert.Collection(layouts,
            layout =>
            {
                Assert.Equal(AdminListConstants.List, layout.Name);
                Assert.Equal("/Admin/Contents/ContentItems?q=post&layout=List", layout.Url);
            },
            layout =>
            {
                Assert.Equal(AdminListConstants.Table, layout.Name);
                Assert.Equal("/Admin/Contents/ContentItems?q=post&layout=Table", layout.Url);
            });
    }

    [Fact]
    public async Task GetLayoutOptionsAsync_AskedTwice_OffersTheSameLayouts()
    {
        var options = new AdminListOptions { AllowUserSelection = true };
        var resolver = CreateResolver(options, new DefaultHttpContext(), AdminListConstants.List, AdminListConstants.Table);

        // A page rendering several lists turns their selector off itself, so asking again changes nothing.
        var first = await resolver.GetLayoutOptionsAsync(TestContext.Current.CancellationToken);
        var second = await resolver.GetLayoutOptionsAsync(TestContext.Current.CancellationToken);

        Assert.Equal(first.Select(layout => layout.Url), second.Select(layout => layout.Url));
        Assert.Equal(2, second.Count);
    }

    [Fact]
    public async Task GetLayoutOptionsAsync_SiteKeepsTheChoice_OffersNothing()
    {
        var resolver = CreateResolver(new AdminListOptions(), new DefaultHttpContext(), AdminListConstants.List, AdminListConstants.Table);

        Assert.Empty(await resolver.GetLayoutOptionsAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetLayoutOptionsAsync_SiteRendersListsOneWay_OffersNothing()
    {
        var options = new AdminListOptions { AllowUserSelection = true };
        var resolver = CreateResolver(options, new DefaultHttpContext(), AdminListConstants.List);

        Assert.Empty(await resolver.GetLayoutOptionsAsync(TestContext.Current.CancellationToken));
    }

    private static DefaultAdminListLayoutResolver CreateResolver(AdminListOptions options)
        => CreateResolver(options, new DefaultHttpContext());

    // The layouts a site can render come from its shape table, so a test says which ones exist.
    private static DefaultAdminListLayoutResolver CreateResolver(AdminListOptions options, HttpContext httpContext, params string[] availableLayouts)
    {
        var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };

        var theme = new Mock<IExtensionInfo>();
        theme.Setup(t => t.Id).Returns("TheAdmin");

        var themeManager = new Mock<IThemeManager>();
        themeManager.Setup(t => t.GetThemeAsync()).ReturnsAsync(theme.Object);

        // The shape table lowercases the shapes of templates, e.g. AdminListLayout-Grid.Option.cshtml.
        var shapeTable = new ShapeTable(
            new Dictionary<string, ShapeDescriptor>(StringComparer.OrdinalIgnoreCase),
            availableLayouts.ToDictionary(
                layout => (AdminListConstants.OptionShapePrefix + layout).ToLowerInvariant(),
                layout => new ShapeBinding { BindingName = (AdminListConstants.OptionShapePrefix + layout).ToLowerInvariant() },
                StringComparer.OrdinalIgnoreCase));

        var shapeTableManager = new Mock<IShapeTableManager>();
        shapeTableManager.Setup(s => s.GetShapeTableAsync(It.IsAny<string>())).ReturnsAsync(shapeTable);

        return new DefaultAdminListLayoutResolver(
            Mock.Of<IOptionsMonitor<AdminListOptions>>(m => m.CurrentValue == options),
            httpContextAccessor,
            new AdminListLayoutPreference(httpContextAccessor),
            themeManager.Object,
            shapeTableManager.Object);
    }

    // The effective defaults are produced by AdminListOptionsConfiguration, which layers the site settings on
    // top of the values bound from appsettings.json.
    private static AdminListOptions Configure(AdminListOptions options, ISite site)
    {
        var siteService = new Mock<ISiteService>();
        siteService.Setup(s => s.GetSiteSettingsAsync()).ReturnsAsync(site);

        new AdminListOptionsConfiguration(siteService.Object).Configure(options);

        return options;
    }
}
