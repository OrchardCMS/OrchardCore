using AngleSharp.Html.Parser;
using OrchardCore.Admin.Models;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Shortcodes.Models;
using OrchardCore.Shortcodes.Services;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Shortcodes;

public class ShortcodesBreadcrumbTests
{
    [Fact]
    public async Task Render_WithoutDashboardFeature_UsesOnlyDeclaredItems()
    {
        using var context = new SiteContext();

        await context.InitializeAsync();

        using var response = await context.Client.GetAsync("Admin/Shortcodes/Create", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        using var document = new HtmlParser().ParseDocument(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        var trail = Assert.Single(document.QuerySelectorAll("nav.oc-breadcrumb"));
        Assert.Equal(["Shortcodes", "Create Shortcode"], trail.QuerySelectorAll(".breadcrumb-item").Select(node => node.TextContent));
    }

    [Theory]
    [InlineData("Admin/Shortcodes", "breadcrumb-shortcodes", "Shortcodes", true)]
    [InlineData("Admin/Shortcodes/Create", "breadcrumb-shortcodes-create", "Create Shortcode", true)]
    [InlineData("Admin/Shortcodes/Edit/breadcrumb", "breadcrumb-shortcodes-edit", "Edit Shortcode", true)]
    [InlineData("Admin/Shortcodes", "breadcrumb-shortcodes", "Shortcodes", false)]
    [InlineData("Admin/Shortcodes/Create", "breadcrumb-shortcodes-create", "Create Shortcode", false)]
    [InlineData("Admin/Shortcodes/Edit/breadcrumb", "breadcrumb-shortcodes-edit", "Edit Shortcode", false)]
    public async Task Render_InlineTrail_PreservesTitlesAndRespectsBreadcrumbSetting(string path, string trailClass, string title, bool showBreadcrumb)
    {
        using var context = new SiteContext();

        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
            var site = await siteService.LoadSiteSettingsAsync();
            site.Put(new AdminSettings { ShowBreadcrumb = showBreadcrumb });
            await siteService.UpdateSiteSettingsAsync(site);

            var templatesManager = scope.ServiceProvider.GetRequiredService<ShortcodeTemplatesManager>();
            await templatesManager.UpdateShortcodeTemplateAsync("breadcrumb", new ShortcodeTemplate
            {
                Content = "Breadcrumb test",
            });

            var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var availableFeatures = await featuresManager.GetAvailableFeaturesAsync();
            await featuresManager.EnableFeaturesAsync(
                availableFeatures.Where(feature => feature.Id == "OrchardCore.AdminDashboard"),
                force: true);
        });

        await context.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);

        using var response = await context.Client.GetAsync(path, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var document = new HtmlParser().ParseDocument(html);

        if (showBreadcrumb)
        {
            var trail = Assert.Single(document.QuerySelectorAll($"nav.oc-breadcrumb.{trailClass}"));
            var nodes = trail.QuerySelectorAll(".breadcrumb-item");
            var isList = path == "Admin/Shortcodes";

            string[] expectedNodes = isList ? ["Dashboard", "Shortcodes"] : ["Dashboard", "Shortcodes", title];
            Assert.Equal(expectedNodes, nodes.Select(node => node.TextContent));

            var dashboardLink = Assert.Single(nodes[0].QuerySelectorAll("a"));
            Assert.Equal($"/{context.TenantName}/Admin", dashboardLink.GetAttribute("href"));

            if (!isList)
            {
                var listLink = Assert.Single(nodes[1].QuerySelectorAll("a"));
                Assert.Equal($"/{context.TenantName}/Admin/Shortcodes", listLink.GetAttribute("href"));
            }

            var current = Assert.Single(trail.QuerySelectorAll("[aria-current='page']"));
            Assert.Equal(title, current.TextContent);
            Assert.Empty(current.QuerySelectorAll("a"));
        }
        else
        {
            Assert.Empty(document.QuerySelectorAll("nav.oc-breadcrumb"));
        }

        var heading = Assert.Single(document.QuerySelectorAll("h1.oc-breadcrumb-title"));
        Assert.Equal(title, heading.TextContent);
        Assert.Equal($"Test Site - {title}", document.Title);
        Assert.Empty(document.QuerySelectorAll("breadcrumb, breadcrumb-item"));
    }
}
