using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents;
using OrchardCore.Contents.Security;
using OrchardCore.Entities;
using OrchardCore.Menu.Models;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Taxonomies.Models;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Contents;

public class InlineContentBreadcrumbTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AddParts_ListPermission_RendersParentLinkOnlyWhenAuthorized(bool canViewTypes)
    {
        List<Permission> permissions = [AdminPermissions.AccessAdminPanel, ContentTypesPermissions.EditContentTypes];

        if (canViewTypes)
        {
            permissions.Add(ContentTypesPermissions.ViewContentTypes);
        }

        using var context = new SiteContext().WithPermissionsContext(new PermissionsContext
        {
            UsePermissionsContext = true,
            AuthorizedPermissions = permissions,
        });

        await context.InitializeAsync();

        using var document = await GetAdminPageAsync(context, "Admin/ContentTypes/AddPartsTo/Article");

        AssertTitle(document, "Add Parts");
        var trail = Assert.Single(document.QuerySelectorAll("nav.oc-breadcrumb"));
        var list = Assert.Single(trail.QuerySelectorAll(".breadcrumb-item"), item => item.TextContent == "Content Types");

        if (canViewTypes)
        {
            AssertLink(trail, "Content Types", "/Admin/ContentTypes/List");
        }
        else
        {
            Assert.Empty(list.QuerySelectorAll("a"));
        }

        AssertLink(trail, "Edit Content Type - Article", "/Admin/ContentTypes/Edit/Article");
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", true)]
    [InlineData("Article", true)]
    [InlineData("Paragraph", false)]
    public async Task Create_ListPermission_RequiresGlobalOrListableTypeAccess(string listPermissionContentType, bool canList)
    {
        List<Permission> permissions = [AdminPermissions.AccessAdminPanel, CommonPermissions.EditContent];

        if (listPermissionContentType is not null)
        {
            permissions.Add(listPermissionContentType.Length == 0
                ? CommonPermissions.ListContent
                : ContentTypePermissionsHelper.CreateDynamicPermission(
                    ContentTypePermissionsHelper.ConvertToDynamicPermission(CommonPermissions.ListContent),
                    listPermissionContentType));
        }

        using var context = new SiteContext().WithPermissionsContext(new PermissionsContext
        {
            UsePermissionsContext = true,
            AuthorizedPermissions = permissions,
        });

        await context.InitializeAsync();

        using var document = await GetAdminPageAsync(context, "Admin/Contents/ContentTypes/Article/Create");

        AssertTitle(document, "New Article");
        var trail = Assert.Single(document.QuerySelectorAll("nav.oc-breadcrumb"));
        var list = Assert.Single(trail.QuerySelectorAll(".breadcrumb-item"), item => item.TextContent == "Manage Content");

        if (canList)
        {
            AssertLink(trail, "Manage Content", "/Admin/Contents/ContentItems");
        }
        else
        {
            Assert.Empty(list.QuerySelectorAll("a"));
        }
    }

    [Theory]
    [InlineData("Article", true)]
    [InlineData("Article", false)]
    [InlineData("Paragraph", true)]
    [InlineData("Paragraph", false)]
    public async Task Display_EscapedContentTitle_EncodesExactlyOnce(string contentType, bool showBreadcrumb)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await SetShowBreadcrumbAsync(context, showBreadcrumb);

        const string title = "A & B &amp; <title> \"quoted\"";
        var contentItemId = string.Empty;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();
            var contentItem = await contentManager.NewAsync(contentType);
            contentItem.DisplayText = title;
            await contentManager.CreateAsync(contentItem, VersionOptions.Published);
            contentItemId = contentItem.ContentItemId;
        });

        using var document = await GetAdminPageAsync(context, $"Admin/Contents/ContentItems/{contentItemId}/Display");

        AssertTitle(document, title);

        if (showBreadcrumb)
        {
            var trail = Assert.Single(document.QuerySelectorAll("nav.oc-breadcrumb"));
            AssertLink(trail, "Manage Content", "/Admin/Contents/ContentItems");
            var current = Assert.Single(trail.QuerySelectorAll("[aria-current='page']"));
            Assert.Equal(title, current.TextContent);
            Assert.Empty(current.Children);
        }
        else
        {
            Assert.Empty(document.QuerySelectorAll("nav.oc-breadcrumb"));
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Render_FormattedAndFilteredTitles_EncodesExactlyOnce(bool showBreadcrumb)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await SetShowBreadcrumbAsync(context, showBreadcrumb);

        const string displayName = "Article & &amp; <name> \"{0}\"";

        await context.UsingTenantScopeAsync(async scope =>
        {
            var contentDefinitionManager = scope.ServiceProvider.GetRequiredService<IContentDefinitionManager>();
            await contentDefinitionManager.AlterTypeDefinitionAsync("Article", builder => builder.WithDisplayName(displayName));
        });

        foreach (var (path, title) in new[]
        {
            ("Admin/Contents/ContentTypes/Article/Create", $"New {displayName}"),
            ("Admin/Contents/ContentItems/Article", displayName),
            ("Admin/ContentTypes/Edit/Article", $"Edit Content Type - {displayName}"),
        })
        {
            using var document = await GetAdminPageAsync(context, path);

            AssertTitle(document, title);

            if (showBreadcrumb)
            {
                var trail = Assert.Single(document.QuerySelectorAll("nav.oc-breadcrumb"));
                var current = Assert.Single(trail.QuerySelectorAll("[aria-current='page']"));
                Assert.Equal(title, current.TextContent);
                Assert.Empty(current.Children);
            }
            else
            {
                Assert.Empty(document.QuerySelectorAll("nav.oc-breadcrumb"));
            }
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AddParts_BreadcrumbSetting_PreservesTypeParentAndTitle(bool showBreadcrumb)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await SetShowBreadcrumbAsync(context, showBreadcrumb);

        using var document = await GetAdminPageAsync(context, "Admin/ContentTypes/AddPartsTo/Article");

        AssertTitle(document, "Add Parts");

        if (showBreadcrumb)
        {
            var trail = Assert.Single(document.QuerySelectorAll("nav.oc-breadcrumb"));
            AssertLink(trail, "Content Types", "/Admin/ContentTypes/List");
            AssertLink(trail, "Edit Content Type - Article", "/Admin/ContentTypes/Edit/Article");
            Assert.Equal("Add Parts", Assert.Single(trail.QuerySelectorAll("[aria-current='page']")).TextContent);
        }
        else
        {
            Assert.Empty(document.QuerySelectorAll("nav.oc-breadcrumb"));
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AddField_BreadcrumbSetting_PreservesPartParentAndTitle(bool showBreadcrumb)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await SetShowBreadcrumbAsync(context, showBreadcrumb);

        using var document = await GetAdminPageAsync(context, "Admin/ContentTypes/AddFieldsTo/Article");

        AssertTitle(document, "Add New Field To \"Article\"");

        if (showBreadcrumb)
        {
            var trail = Assert.Single(document.QuerySelectorAll("nav.oc-breadcrumb"));
            AssertLink(trail, "Content Parts", "/Admin/ContentTypes/ListParts");
            AssertLink(trail, "Edit Content Part - Article", "/Admin/ContentParts/Edit/Article");
        }
        else
        {
            Assert.Empty(document.QuerySelectorAll("nav.oc-breadcrumb"));
        }
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(true, false)]
    public async Task NestedEditors_BreadcrumbSetting_PreservesParentLinksAndTitles(bool taxonomy, bool showBreadcrumb)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await SetShowBreadcrumbAsync(context, showBreadcrumb);

        const string parentDisplayText = "Parent & <name>";
        var parentId = string.Empty;
        var childId = string.Empty;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();
            var parent = await contentManager.NewAsync(taxonomy ? "Taxonomy" : "Menu");
            var child = await contentManager.NewAsync(taxonomy ? "Category" : "LinkMenuItem");
            parent.DisplayText = parentDisplayText;

            if (taxonomy)
            {
                parent.Alter<TaxonomyPart>(part =>
                {
                    part.TermContentType = "Category";
                    part.Terms.Add(child);
                });
            }
            else
            {
                parent.Alter<MenuItemsListPart>(part => part.MenuItems.Add(child));
            }

            await contentManager.CreateAsync(parent, VersionOptions.Published);
            parentId = parent.ContentItemId;
            childId = child.ContentItemId;
        });

        var createPath = taxonomy
            ? $"Admin/Taxonomies/Create/Category?taxonomyContentItemId={parentId}"
            : $"Admin/Menu/Create/LinkMenuItem?menuContentItemId={parentId}";
        var editPath = taxonomy
            ? $"Admin/Taxonomies/Edit/{parentId}/{childId}"
            : $"Admin/Menu/Edit?menuContentItemId={parentId}&menuItemId={childId}";
        var typeDisplayName = taxonomy ? "Category" : "Link Menu Item";

        foreach (var (path, title) in new[] { (createPath, $"New {typeDisplayName}"), (editPath, $"Edit {typeDisplayName}") })
        {
            using var document = await GetAdminPageAsync(context, path);

            AssertTitle(document, title);

            if (showBreadcrumb)
            {
                var trail = Assert.Single(document.QuerySelectorAll("nav.oc-breadcrumb"));
                AssertLink(trail, "Manage Content", "/Admin/Contents/ContentItems");
                AssertLink(trail, $"Edit {parentDisplayText}", $"/Admin/Contents/ContentItems/{parentId}/Edit");
                Assert.Equal(title, Assert.Single(trail.QuerySelectorAll("[aria-current='page']")).TextContent);
            }
            else
            {
                Assert.Empty(document.QuerySelectorAll("nav.oc-breadcrumb"));
            }
        }
    }

    private static Task SetShowBreadcrumbAsync(SiteContext context, bool showBreadcrumb)
        => context.UsingTenantScopeAsync(async scope =>
        {
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
            var site = await siteService.LoadSiteSettingsAsync();
            site.Put(new AdminSettings { ShowBreadcrumb = showBreadcrumb });
            await siteService.UpdateSiteSettingsAsync(site);
        });

    private static async Task<IDocument> GetAdminPageAsync(SiteContext context, string path)
    {
        using var response = await context.Client.GetAsync(path, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        return new HtmlParser().ParseDocument(html);
    }

    private static void AssertTitle(IDocument document, string title)
    {
        Assert.Equal(title, Assert.Single(document.QuerySelectorAll("h1.oc-breadcrumb-title")).TextContent);
        Assert.Equal($"Test Site - {title}", document.Title);
        Assert.Empty(document.QuerySelectorAll("breadcrumb, breadcrumb-item"));
    }

    private static void AssertLink(IElement trail, string text, string path)
    {
        var link = Assert.Single(trail.QuerySelectorAll("a"), link => link.TextContent == text);
        Assert.EndsWith(path, link.GetAttribute("href"));
    }
}
