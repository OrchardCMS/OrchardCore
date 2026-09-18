using AngleSharp.Html.Parser;
using OrchardCore.Environment.Shell;
using OrchardCore.Shortcodes.Models;
using OrchardCore.Shortcodes.Services;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Shortcodes;

public class ShortcodesBreadcrumbTests
{
    [Theory]
    [InlineData("Admin/Shortcodes", "breadcrumb-shortcodes", "Shortcodes")]
    [InlineData("Admin/Shortcodes/Create", "breadcrumb-shortcodes-create", "Create Shortcode")]
    [InlineData("Admin/Shortcodes/Edit/breadcrumb", "breadcrumb-shortcodes-edit", "Edit Shortcode")]
    public async Task Render_InlineTrail_PreservesNavigationAndDashboardExtension(string path, string trailClass, string title)
    {
        using var context = new SiteContext();

        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
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

        var heading = Assert.Single(document.QuerySelectorAll("h1.oc-breadcrumb-title"));
        Assert.Equal(title, heading.TextContent);
        Assert.Equal($"Test Site - {title}", document.Title);
        Assert.Empty(document.QuerySelectorAll("breadcrumb, breadcrumb-item"));
    }
}
