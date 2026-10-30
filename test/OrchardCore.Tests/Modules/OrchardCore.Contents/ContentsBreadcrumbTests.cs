using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;
using OrchardCore.Contents.Security;
using OrchardCore.Entities;
using OrchardCore.Security.Permissions;
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

        using var document = new HtmlParser().ParseDocument(html);
        var trail = Assert.Single(document.QuerySelectorAll("nav.oc-breadcrumb"));
        AssertLink(trail, "Manage Content", "/Admin/Contents/ContentItems");
        AssertLink(trail, "Article", "/Admin/Contents/ContentItems/Article");
        Assert.Equal("Edit Article", Assert.Single(trail.QuerySelectorAll("[aria-current='page']")).TextContent);
    }

    [Fact]
    public async Task Edit_LocalReturnUrl_LinksTheContentTypeToTheReturnUrl()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        var contentItemId = await CreateArticleAsync(context);
        const string returnUrl = "/Admin/Contents/ContentItems/Article?options.displayText=Hello+%26+World";

        using var document = await GetAdminDocumentAsync(context, EditPath(contentItemId, returnUrl));
        var trail = Assert.Single(document.QuerySelectorAll("nav.oc-breadcrumb"));

        AssertLink(trail, "Manage Content", "/Admin/Contents/ContentItems");
        AssertLink(trail, "Article", returnUrl);
        Assert.Equal("Edit Article", Assert.Single(trail.QuerySelectorAll("[aria-current='page']")).TextContent);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("https://example.com/admin")]
    [InlineData("//example.com/admin")]
    public async Task Edit_NonLocalReturnUrl_LinksToTheFilteredContentList(string returnUrl)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        var contentItemId = await CreateArticleAsync(context);

        using var document = await GetAdminDocumentAsync(context, EditPath(contentItemId, returnUrl));
        var trail = Assert.Single(document.QuerySelectorAll("nav.oc-breadcrumb"));

        AssertLink(trail, "Article", "/Admin/Contents/ContentItems/Article");
    }

    [Fact]
    public async Task Edit_NonListableType_OmitsTheContentTypeAncestor()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        var contentItemId = await CreateContentItemAsync(context, "BlogPost");
        const string returnUrl = "/Admin/Contents/ContentItems/4blogpost0000000000000000/Edit";

        using var document = await GetAdminDocumentAsync(context, EditPath(contentItemId, returnUrl));
        var trail = Assert.Single(document.QuerySelectorAll("nav.oc-breadcrumb"));
        var items = trail.QuerySelectorAll(".breadcrumb-item").Select(item => item.TextContent).ToArray();

        Assert.Equal(["Manage Content", "Edit Blog Post"], items);
        AssertLink(trail, "Manage Content", "/Admin/Contents/ContentItems");
        Assert.DoesNotContain(trail.QuerySelectorAll("a"), link => link.GetAttribute("href") == returnUrl);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", true)]
    [InlineData("Article", true)]
    [InlineData("Page", false)]
    public async Task Edit_ListPermission_LinksTheContentTypeOnlyWhenAuthorized(string listPermissionContentType, bool canListType)
    {
        List<Permission> permissions = [AdminPermissions.AccessAdminPanel, CommonPermissions.EditContent];

        if (listPermissionContentType is not null)
        {
            permissions.Add(listPermissionContentType.Length == 0
                ? CommonPermissions.ListContent
                : ContentTypePermissionsHelper.CreateDynamicPermissionOf(CommonPermissions.ListContent.Name, listPermissionContentType));
        }

        using var context = new SiteContext().WithPermissionsContext(new PermissionsContext
        {
            UsePermissionsContext = true,
            AuthorizedPermissions = permissions,
        });

        await context.InitializeAsync();

        var contentItemId = await CreateArticleAsync(context);

        using var document = await GetAdminDocumentAsync(context, $"Admin/Contents/ContentItems/{contentItemId}/Edit");
        var trail = Assert.Single(document.QuerySelectorAll("nav.oc-breadcrumb"));
        var type = Assert.Single(trail.QuerySelectorAll(".breadcrumb-item"), item => item.TextContent == "Article");

        if (canListType)
        {
            AssertLink(trail, "Article", "/Admin/Contents/ContentItems/Article");
        }
        else
        {
            Assert.Empty(type.QuerySelectorAll("a"));
        }
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

    private static Task<string> CreateArticleAsync(SiteContext context)
        => CreateContentItemAsync(context, "Article");

    private static async Task<string> CreateContentItemAsync(SiteContext context, string contentType)
    {
        var contentItemId = string.Empty;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();

            var contentItem = await contentManager.NewAsync(contentType);
            contentItem.DisplayText = "A breadcrumb article";

            await contentManager.CreateAsync(contentItem);

            contentItemId = contentItem.ContentItemId;
        });

        return contentItemId;
    }

    private static string EditPath(string contentItemId, string returnUrl)
    {
        var path = $"Admin/Contents/ContentItems/{contentItemId}/Edit";

        return returnUrl is null
            ? path
            : path + "?returnUrl=" + Uri.EscapeDataString(returnUrl);
    }

    private static async Task<IDocument> GetAdminDocumentAsync(SiteContext context, string path)
    {
        var html = await GetAdminPageAsync(context, path);

        return new HtmlParser().ParseDocument(html);
    }

    private static void AssertLink(IElement trail, string text, string path)
    {
        var link = Assert.Single(trail.QuerySelectorAll("a"), link => link.TextContent == text);
        Assert.EndsWith(path, link.GetAttribute("href"));
    }
}
