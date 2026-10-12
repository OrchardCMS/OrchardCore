using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Indexing;
using OrchardCore.Queries;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Queries;

public class SearchAndSettingsInlineBreadcrumbTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task QueryPages_BreadcrumbSetting_PreservesParentLinksAndEscapedTitles(bool showBreadcrumb)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await SetShowBreadcrumbAsync(context, showBreadcrumb);

        const string queryName = "Reports & <name> \"quoted\" 'single' &lt;literal&gt;";

        await context.UsingTenantScopeAsync(async scope =>
        {
            var queryManager = scope.ServiceProvider.GetRequiredService<IQueryManager>();
            var query = await queryManager.NewAsync("Sql");
            Assert.NotNull(query);
            query.Name = queryName;
            await queryManager.SaveAsync(query);
        });

        var pages = new[]
        {
            (Path: "Admin/Queries/Index", TrailClass: "breadcrumb-queries", Title: "Queries", IsList: true),
            (Path: "Admin/Queries/Create/Sql", TrailClass: "breadcrumb-queries-create", Title: "New Sql query", IsList: false),
            (Path: $"Admin/Queries/Edit/{Uri.EscapeDataString(queryName)}", TrailClass: "breadcrumb-queries-edit", Title: $"Edit '{queryName}' query", IsList: false),
            (Path: "Admin/Queries/Sql/Query", TrailClass: "breadcrumb-queries-run", Title: "SQL Query", IsList: false),
        };

        foreach (var (path, trailClass, title, isList) in pages)
        {
            using var document = await GetAdminPageAsync(context, path);

            AssertTitle(document, title);
            AssertTrail(document, showBreadcrumb, trailClass, isList ? ["Queries"] : ["Queries", title]);

            if (showBreadcrumb && !isList)
            {
                AssertParentLink(document, "Queries", $"/{context.TenantName}/Admin/Queries/Index");
            }
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IndexPages_BreadcrumbSetting_PreservesSourceDisplayNameAndEscapedIndexName(bool showBreadcrumb)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await EnableFeaturesAsync(context, "OrchardCore.Lucene");
        await SetShowBreadcrumbAsync(context, showBreadcrumb);

        const string indexName = "Index & <name> \"quoted\" 'single' &lt;literal&gt;";
        var indexId = string.Empty;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var indexManager = scope.ServiceProvider.GetRequiredService<IIndexProfileManager>();
            var index = await indexManager.NewAsync("Lucene", "Content");
            Assert.NotNull(index);
            index.Name = indexName;
            index.IndexName = "breadcrumb-index";
            index.IndexFullName = "breadcrumb-index";
            await indexManager.CreateAsync(index);
            indexId = index.Id;
        });

        var pages = new[]
        {
            (Path: "Admin/indexing", TrailClass: "breadcrumb-indexes", Title: "Indexes", IsList: true),
            (Path: "Admin/indexing/create/Lucene/Content", TrailClass: "breadcrumb-indexes-create", Title: "New 'Content in Lucene' index", IsList: false),
            (Path: $"Admin/indexing/edit/{indexId}", TrailClass: "breadcrumb-indexes-edit", Title: $"Edit '{indexName}' index", IsList: false),
        };

        foreach (var (path, trailClass, title, isList) in pages)
        {
            using var document = await GetAdminPageAsync(context, path);

            AssertTitle(document, title);
            AssertTrail(document, showBreadcrumb, trailClass, isList ? ["Indexes"] : ["Indexes", title]);

            if (showBreadcrumb && !isList)
            {
                AssertParentLink(document, "Indexes", $"/{context.TenantName}/Admin/indexing");
            }
        }

        using var queryDocument = await GetAdminPageAsync(context, "Admin/Lucene/query");
        AssertTitle(queryDocument, "Lucene Query");
        AssertTrail(queryDocument, showBreadcrumb, "breadcrumb-lucene-query", ["Lucene Query"]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SettingsPages_BreadcrumbSetting_PreservesLiteralTrailNamesAndTitles(bool showBreadcrumb)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await EnableFeaturesAsync(context, "OrchardCore.Cors", "OrchardCore.Email");
        await SetShowBreadcrumbAsync(context, showBreadcrumb);

        var pages = new[]
        {
            (Path: "Admin/Cors", TrailClass: "breadcrumb-cors-settings", Title: "CORS Settings"),
            (Path: "Admin/Email/Test", TrailClass: "breadcrumb-email-settings", Title: "Email"),
            (Path: "Admin/Features", TrailClass: "breadcrumb-features", Title: "Features"),
            (Path: "Admin/Recipes", TrailClass: "breadcrumb-recipes", Title: "Recipes"),
            (Path: "Admin/Settings/general", TrailClass: "breadcrumb-general-settings", Title: "Settings"),
            (Path: "Admin/Themes", TrailClass: "breadcrumb-themes", Title: "Themes"),
        };

        foreach (var (path, trailClass, title) in pages)
        {
            using var document = await GetAdminPageAsync(context, path);

            AssertTitle(document, title);
            AssertTrail(document, showBreadcrumb, trailClass, [title]);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SqlRunner_ManageQueriesPermission_ControlsParentLinkWithoutRemovingText(bool canManageQueries)
    {
        List<Permission> permissions =
        [
            AdminPermissions.AccessAdminPanel,
            QueriesPermissions.ManageSqlQueries,
        ];

        if (canManageQueries)
        {
            permissions.Add(QueryPermissions.ManageQueries);
        }

        using var context = new SiteContext()
            .WithPermissionsContext(new PermissionsContext
            {
                UsePermissionsContext = true,
                AuthorizedPermissions = permissions,
            });

        await context.InitializeAsync();
        await SetShowBreadcrumbAsync(context, true);

        using var document = await GetAdminPageAsync(context, "Admin/Queries/Sql/Query");

        AssertTitle(document, "SQL Query");
        AssertTrail(document, true, "breadcrumb-queries-run", ["Queries", "SQL Query"]);

        var parent = Assert.Single(document.QuerySelectorAll("nav.oc-breadcrumb .breadcrumb-item"),
            node => node.TextContent == "Queries");

        if (canManageQueries)
        {
            AssertParentLink(document, "Queries", $"/{context.TenantName}/Admin/Queries/Index");
        }
        else
        {
            Assert.Empty(parent.QuerySelectorAll("a"));
            Assert.False(parent.HasAttribute("aria-current"));
        }
    }

    private static async Task EnableFeaturesAsync(SiteContext context, params string[] featureIds)
    {
        await context.UsingTenantScopeAsync(async scope =>
        {
            var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var availableFeatures = await featuresManager.GetAvailableFeaturesAsync();
            var features = availableFeatures.Where(feature => featureIds.Contains(feature.Id)).ToArray();
            Assert.Equal(featureIds.Length, features.Length);
            await featuresManager.EnableFeaturesAsync(features, force: true);
        });

        await context.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);
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

        return new HtmlParser().ParseDocument(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    private static void AssertTitle(IDocument document, string title)
    {
        var heading = Assert.Single(document.QuerySelectorAll("h1.oc-breadcrumb-title"));
        Assert.Equal(title, heading.TextContent);
        Assert.Empty(heading.Children);
        Assert.Equal($"Test Site - {title}", document.Title);
        Assert.Empty(document.QuerySelectorAll("breadcrumb, breadcrumb-item"));
    }

    private static void AssertTrail(IDocument document, bool showBreadcrumb, string trailClass, string[] expectedNodes)
    {
        if (!showBreadcrumb)
        {
            Assert.Empty(document.QuerySelectorAll("nav.oc-breadcrumb"));

            return;
        }

        var trail = Assert.Single(document.QuerySelectorAll("nav.oc-breadcrumb"));
        Assert.Contains(trailClass, trail.ClassList);
        Assert.Equal(expectedNodes, trail.QuerySelectorAll(".breadcrumb-item").Select(node => node.TextContent));

        var current = Assert.Single(trail.QuerySelectorAll("[aria-current='page']"));
        Assert.Equal(expectedNodes[^1], current.TextContent);
        Assert.Empty(current.Children);
    }

    private static void AssertParentLink(IDocument document, string text, string href)
    {
        var link = Assert.Single(document.QuerySelectorAll("nav.oc-breadcrumb a"), link => link.TextContent == text);
        Assert.Equal(href, link.GetAttribute("href"));
    }
}
