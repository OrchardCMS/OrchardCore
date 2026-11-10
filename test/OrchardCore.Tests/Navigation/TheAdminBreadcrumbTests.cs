using AngleSharp.Html.Parser;
using OrchardCore.Admin.Models;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Navigation;

public class TheAdminBreadcrumbTests
{
    [Fact]
    public async Task Breadcrumb_TitlesInContent_RendersTrailAboveTitle()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await SetAdminSettingsAsync(context, displayTitlesInTopbar: false);

        using var document = await GetDocumentAsync(context, "Admin/ContentTypes/Edit/Article");

        var trail = Assert.Single(document.QuerySelectorAll(".ta-content nav.oc-breadcrumb"));
        var title = Assert.Single(document.QuerySelectorAll(".ta-content h1.oc-breadcrumb-title"));
        Assert.Same(trail.NextElementSibling, title);
        Assert.Empty(document.QuerySelectorAll(".ta-navbar-top .oc-breadcrumb, .ta-navbar-top .oc-breadcrumb-title"));
    }

    [Fact]
    public async Task Breadcrumb_TitlesInTopbar_KeepsTitleInTopbarAndTrailInContent()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await SetAdminSettingsAsync(context, displayTitlesInTopbar: true);

        using var document = await GetDocumentAsync(context, "Admin/ContentTypes/Edit/Article");

        var title = Assert.Single(document.QuerySelectorAll("h1.oc-breadcrumb-title"));
        Assert.Equal("Edit Content Type - Article", title.TextContent);
        Assert.NotNull(title.Closest(".ta-navbar-top .brand-wrapper-title"));

        var trail = Assert.Single(document.QuerySelectorAll("nav.oc-breadcrumb"));
        Assert.NotNull(trail.Closest(".ta-content"));
        Assert.Equal(
            ["Content Types", "Edit Content Type - Article"],
            trail.QuerySelectorAll(".breadcrumb-item").Select(node => node.TextContent));
    }

    private static Task SetAdminSettingsAsync(SiteContext context, bool displayTitlesInTopbar)
        => context.UsingTenantScopeAsync(async scope =>
        {
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
            var site = await siteService.LoadSiteSettingsAsync();
            site.Put(new AdminSettings { ShowBreadcrumb = true, DisplayTitlesInTopbar = displayTitlesInTopbar });
            await siteService.UpdateSiteSettingsAsync(site);
        });

    private static async Task<AngleSharp.Html.Dom.IHtmlDocument> GetDocumentAsync(SiteContext context, string path)
    {
        using var response = await context.Client.GetAsync(path, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        return new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }
}
