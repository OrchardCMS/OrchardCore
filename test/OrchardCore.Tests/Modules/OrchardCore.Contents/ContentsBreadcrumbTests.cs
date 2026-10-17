using AngleSharp.Html.Parser;
using OrchardCore.Admin.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Contents;

public class ContentsBreadcrumbTests
{
    [Fact]
    public async Task List_RendersTheBreadcrumbOfTheContentItemsList()
    {
        using var context = new SiteContext();

        await context.InitializeAsync();

        var html = await GetAdminPageAsync(context, "Admin/Contents/ContentItems");

        Assert.Contains("oc-breadcrumb", html);
        Assert.Contains("breadcrumb-contents", html);

        // The list is the page itself, so the trail renders its text as the breadcrumb title heading.
        Assert.Contains("oc-breadcrumb-title\">Manage Content</h1>", html);
    }

    [Fact]
    public async Task Edit_RendersTheContentItemsListAsTheParentOfThePage()
    {
        using var context = new SiteContext();

        await context.InitializeAsync();

        var contentItemId = await CreateArticleAsync(context);

        var html = await GetAdminPageAsync(context, $"Admin/Contents/ContentItems/{contentItemId}/Edit");

        Assert.Contains("breadcrumb-contents-edit", html);

        // The list is an ancestor of the page, so its node is a link.
        Assert.Contains("Manage Content</a>", html);

        // The page itself is the current node, and the trail renders its text as the breadcrumb title heading.
        Assert.Contains("oc-breadcrumb-title\">Edit Article</h1>", html);
    }

    [Fact]
    public async Task Edit_DisabledBreadcrumb_PreservesInlineTitleWithoutTrail()
    {
        using var context = new SiteContext();

        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
            var site = await siteService.LoadSiteSettingsAsync();
            site.Put(new AdminSettings { ShowBreadcrumb = false });
            await siteService.UpdateSiteSettingsAsync(site);
        });

        var contentItemId = await CreateArticleAsync(context);
        var html = await GetAdminPageAsync(context, $"Admin/Contents/ContentItems/{contentItemId}/Edit");
        using var document = new HtmlParser().ParseDocument(html);

        Assert.Empty(document.QuerySelectorAll("nav.oc-breadcrumb"));
        Assert.Equal("Edit Article", Assert.Single(document.QuerySelectorAll("h1.oc-breadcrumb-title")).TextContent);
        Assert.Equal("Test Site - Edit Article", document.Title);
    }

    private static async Task<string> GetAdminPageAsync(SiteContext context, string path)
    {
        var response = await context.Client.GetAsync(path, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    }

    private static async Task<string> CreateArticleAsync(SiteContext context)
    {
        var contentItemId = string.Empty;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();

            var contentItem = await contentManager.NewAsync("Article");
            contentItem.DisplayText = "A breadcrumb article";

            await contentManager.CreateAsync(contentItem);

            contentItemId = contentItem.ContentItemId;
        });

        return contentItemId;
    }
}
