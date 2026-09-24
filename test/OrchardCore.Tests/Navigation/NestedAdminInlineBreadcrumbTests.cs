using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.WebUtilities;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.AdminMenu.AdminNodes;
using OrchardCore.AdminMenu.Services;
using OrchardCore.Deployment;
using OrchardCore.Deployment.Steps;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;
using OrchardCore.Rules;
using OrchardCore.Rules.Models;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Workflows.Models;
using AdminMenuModel = OrchardCore.AdminMenu.Models.AdminMenu;
using ISession = YesSql.ISession;

namespace OrchardCore.Tests.Navigation;

public class NestedAdminInlineBreadcrumbTests
{
    private const string EscapedName = "Parent & <name> \"quoted\" 'single' &lt;literal&gt;";

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AdminMenuNodes_BreadcrumbSetting_PreservesParentNameAndEscapedTitles(bool showBreadcrumb)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await EnableFeaturesAsync(context, "OrchardCore.AdminMenu");
        await SetShowBreadcrumbAsync(context, showBreadcrumb);

        var node = new LinkAdminNode { LinkText = "Breadcrumb test", LinkUrl = "~/" };
        var menu = new AdminMenuModel
        {
            Name = EscapedName,
            Enabled = false,
            MenuItems = [node],
        };

        await context.UsingTenantScopeAsync(async scope =>
        {
            await scope.ServiceProvider.GetRequiredService<IAdminMenuService>().SaveAsync(menu);
        });

        var menuLink = $"Admin/AdminMenu/Node/List?id={menu.Id}";
        var nodesTitle = $"Edit Nodes for '{EscapedName}'";

        await AssertPageAsync(context, $"Admin/AdminMenu/Edit/{menu.Id}", showBreadcrumb,
            "breadcrumb-admin-menus-edit", $"Edit Admin Menu: {EscapedName}",
            ("Admin Menus", "Admin/AdminMenu/List"));
        await AssertPageAsync(context, menuLink, showBreadcrumb,
            "breadcrumb-admin-menus-nodes", nodesTitle,
            ("Admin Menus", "Admin/AdminMenu/List"));
        await AssertPageAsync(context, $"Admin/AdminMenu/Node/Create?id={menu.Id}&type=LinkAdminNode", showBreadcrumb,
            "breadcrumb-admin-menus-node-create", "Create Node",
            ("Admin Menus", "Admin/AdminMenu/List"), (nodesTitle, menuLink));
        await AssertPageAsync(context, $"Admin/AdminMenu/Node/Edit?id={menu.Id}&treeNodeId={node.UniqueId}", showBreadcrumb,
            "breadcrumb-admin-menus-node-edit", "Edit Node",
            ("Admin Menus", "Admin/AdminMenu/List"), (nodesTitle, menuLink));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task DeploymentSteps_ExportPermissionAndBreadcrumbSetting_PreservesFactoryTitleAndParentText(
        bool showBreadcrumb,
        bool canExport)
    {
        List<Permission> permissions =
        [
            AdminPermissions.AccessAdminPanel,
            DeploymentPermissions.ManageDeploymentPlan,
        ];

        if (canExport)
        {
            permissions.Add(DeploymentPermissions.Export);
        }

        using var context = new SiteContext()
            .WithPermissionsContext(new PermissionsContext
            {
                UsePermissionsContext = true,
                AuthorizedPermissions = permissions,
            });

        await context.InitializeAsync();
        await EnableFeaturesAsync(context, "OrchardCore.Deployment");
        await SetShowBreadcrumbAsync(context, showBreadcrumb);

        var step = new CustomFileDeploymentStep
        {
            Id = "breadcrumb-step",
            FileName = "breadcrumb.txt",
            FileContent = "Breadcrumb test",
        };
        Assert.Null(step.Title);

        var plan = new DeploymentPlan { Name = EscapedName, DeploymentSteps = [step] };
        await context.UsingTenantScopeAsync(async scope =>
        {
            await scope.ServiceProvider.GetRequiredService<ISession>().SaveAsync(plan);
        });

        var listLink = canExport ? "Admin/DeploymentPlan/Index" : null;
        var planLink = canExport ? $"Admin/DeploymentPlan/Display/{plan.Id}" : null;

        await AssertPageAsync(context, $"Admin/DeploymentPlan/{plan.Id}/Step/Create?type=CustomFileDeploymentStep", showBreadcrumb,
            "breadcrumb-deployment-steps-create", "Custom File",
            ("Deployment Plans", listLink), (EscapedName, planLink));
        await AssertPageAsync(context, $"Admin/DeploymentPlan/{plan.Id}/Step/{step.Id}/Edit", showBreadcrumb,
            "breadcrumb-deployment-steps-edit", "Custom File",
            ("Deployment Plans", listLink), (EscapedName, planLink));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WorkflowEditors_BreadcrumbSetting_PreservesWorkflowNameAndNestedLinks(bool showBreadcrumb)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await EnableFeaturesAsync(context, "OrchardCore.Workflows");
        await SetShowBreadcrumbAsync(context, showBreadcrumb);

        var activity = new ActivityRecord { ActivityId = "breadcrumb-activity", Name = "NotifyTask" };
        var workflowType = new WorkflowType
        {
            WorkflowTypeId = Guid.NewGuid().ToString("n"),
            Name = EscapedName,
            Activities = [activity],
        };

        await context.UsingTenantScopeAsync(async scope =>
        {
            await scope.ServiceProvider.GetRequiredService<ISession>().SaveAsync(workflowType);
        });

        var workflowLink = $"Admin/Workflows/Types/Edit/{workflowType.Id}";

        await AssertPageAsync(context, workflowLink, showBreadcrumb,
            "breadcrumb-workflow-types-edit", EscapedName,
            ("Workflows", "Admin/Workflows/Types"));
        await AssertPageAsync(context, $"Admin/Workflows/Types/{workflowType.Id}/Activity/NotifyTask/Add", showBreadcrumb,
            "breadcrumb-workflows-activity-create", "Add Notify Task",
            ("Workflows", "Admin/Workflows/Types"), (EscapedName, workflowLink));
        await AssertPageAsync(context, $"Admin/Workflows/Types/{workflowType.Id}/Activity/{activity.ActivityId}/Edit", showBreadcrumb,
            "breadcrumb-workflows-activity-edit", "Edit Notify Task",
            ("Workflows", "Admin/Workflows/Types"), (EscapedName, workflowLink));
        await AssertPageAsync(context, $"Admin/Workflows/Types/{workflowType.Id}/Instances/Index", showBreadcrumb,
            "breadcrumb-workflow-instances", "Instances",
            ("Workflows", "Admin/Workflows/Types"), (EscapedName, workflowLink));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SitemapSources_BreadcrumbSetting_PreservesEscapedSitemapNameAndParentRoute(bool showBreadcrumb)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await EnableFeaturesAsync(context, "OrchardCore.Sitemaps");
        await SetShowBreadcrumbAsync(context, showBreadcrumb);

        var source = new CustomPathSitemapSource { Id = "breadcrumb-source", Path = "breadcrumb" };
        var sitemap = new Sitemap
        {
            SitemapId = Guid.NewGuid().ToString("n"),
            Name = EscapedName,
            Path = "breadcrumb.xml",
            Enabled = false,
            SitemapSources = [source],
        };

        await context.UsingTenantScopeAsync(async scope =>
        {
            await scope.ServiceProvider.GetRequiredService<ISitemapManager>().UpdateSitemapAsync(sitemap);
        });

        var sitemapLink = $"Admin/Sitemaps/Display/{sitemap.SitemapId}";

        await AssertPageAsync(context, $"Admin/SitemapSource/Create/{sitemap.SitemapId}/CustomPathSitemapSource", showBreadcrumb,
            "breadcrumb-sitemaps-source-create", "Create Sitemap Source",
            ("Sitemaps", "Admin/Sitemaps/List"), (EscapedName, sitemapLink));
        await AssertPageAsync(context, $"Admin/SitemapSource/Edit/{sitemap.SitemapId}/{source.Id}", showBreadcrumb,
            "breadcrumb-sitemaps-source-edit", "Edit Sitemap Source",
            ("Sitemaps", "Admin/Sitemaps/List"), (EscapedName, sitemapLink));
        await AssertPageAsync(context, "Admin/SitemapIndexes/Create", showBreadcrumb,
            "breadcrumb-sitemap-indexes-create", "Create Sitemap Index",
            ("Sitemap Indexes", "Admin/SitemapIndexes/List"));
        await AssertPageAsync(context, "Admin/SitemapsCache/List", showBreadcrumb,
            "breadcrumb-sitemap-cache", "Sitemaps Cache");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task LayerRules_BreadcrumbSetting_PreservesEscapedLayerNameAndQueryRoute(bool showBreadcrumb)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await EnableFeaturesAsync(context, "OrchardCore.Layers");
        await SetShowBreadcrumbAsync(context, showBreadcrumb);

        var condition = new BooleanCondition { ConditionId = "breadcrumb-condition" };
        var layer = new Layer
        {
            Name = EscapedName,
            LayerRule = new Rule
            {
                ConditionId = "breadcrumb-rule",
                Conditions = [condition],
            },
        };

        await context.UsingTenantScopeAsync(async scope =>
        {
            var layerService = scope.ServiceProvider.GetRequiredService<ILayerService>();
            var layers = await layerService.LoadLayersAsync();
            layers.Layers.Add(layer);
            await layerService.UpdateAsync(layers);
        });

        var layerLink = QueryHelpers.AddQueryString("Admin/Layers/Edit", "name", EscapedName);
        var createLink = QueryHelpers.AddQueryString(
            "Admin/Layers/Rules/Create?type=BooleanCondition&conditionGroupId=breadcrumb-rule", "name", EscapedName);
        var editLink = QueryHelpers.AddQueryString(
            "Admin/Layers/Rules/Edit?conditionId=breadcrumb-condition", "name", EscapedName);
        var layerTitle = $"Edit Layer - {EscapedName}";

        await AssertPageAsync(context, layerLink, showBreadcrumb,
            "breadcrumb-layers-edit", layerTitle,
            ("Widgets and Layers", "Admin/Layers"));
        await AssertPageAsync(context, createLink, showBreadcrumb,
            "breadcrumb-layers-rule-create", "Create Rule",
            ("Widgets and Layers", "Admin/Layers"), (layerTitle, layerLink));
        await AssertPageAsync(context, editLink, showBreadcrumb,
            "breadcrumb-layers-rule-edit", "Edit Rule",
            ("Widgets and Layers", "Admin/Layers"), (layerTitle, layerLink));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Limiters_BreadcrumbSetting_PreservesEscapedPolicyNameAndLimiterTitles(bool showBreadcrumb)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await EnableFeaturesAsync(context, "OrchardCore.RateLimits");
        await SetShowBreadcrumbAsync(context, showBreadcrumb);

        var limiter = new RateLimitLimiter { Id = "breadcrumb-limiter", Source = "FixedWindow" };
        var policy = new RateLimitPolicy
        {
            PolicyId = Guid.NewGuid().ToString("n"),
            Name = EscapedName,
            IsEnabled = false,
            Limiters = [limiter],
        };

        await context.UsingTenantScopeAsync(async scope =>
        {
            await scope.ServiceProvider.GetRequiredService<IRateLimitPolicyStore>().CreateAsync(policy);
        });

        var policyLink = $"Admin/RateLimits/Edit/{policy.PolicyId}";

        await AssertPageAsync(context, $"Admin/RateLimits/Limiter/Create/{policy.PolicyId}?id=FixedWindow", showBreadcrumb,
            "breadcrumb-rate-limits-limiter-create", "Add 'Fixed window' Limiter",
            ("Rate Limit Policies", "Admin/RateLimits"), (EscapedName, policyLink));
        await AssertPageAsync(context, $"Admin/RateLimits/Limiter/Edit/{policy.PolicyId}/{limiter.Id}", showBreadcrumb,
            "breadcrumb-rate-limits-limiter-edit", "Edit 'Fixed window' Limiter",
            ("Rate Limit Policies", "Admin/RateLimits"), (EscapedName, policyLink));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RemoteDeployment_BreadcrumbSetting_PreservesSeparateInstanceAndClientTrails(bool showBreadcrumb)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await EnableFeaturesAsync(context, "OrchardCore.Deployment.Remote");
        await SetShowBreadcrumbAsync(context, showBreadcrumb);

        await AssertPageAsync(context, "Admin/Deployment/RemoteInstance/Create", showBreadcrumb,
            "breadcrumb-remote-instances-create", "Create Remote Instance",
            ("Remote Instances", "Admin/Deployment/RemoteInstance/Index"));
        await AssertPageAsync(context, "Admin/Deployment/RemoteClient/Index", showBreadcrumb,
            "breadcrumb-remote-clients", "Remote Clients");
        await AssertPageAsync(context, "Admin/Deployment/RemoteInstance/Index", showBreadcrumb,
            "breadcrumb-remote-instances", "Remote Instances");
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

    private static async Task AssertPageAsync(
        SiteContext context,
        string path,
        bool showBreadcrumb,
        string trailClass,
        string title,
        params (string Text, string Path)[] parents)
    {
        using var response = await context.Client.GetAsync(path, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        using var document = new HtmlParser().ParseDocument(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        var heading = Assert.Single(document.QuerySelectorAll("h1.oc-breadcrumb-title"));
        Assert.Equal(title, heading.TextContent);
        Assert.Empty(heading.Children);
        Assert.Equal($"Test Site - {title}", document.Title);
        Assert.Empty(document.QuerySelectorAll("breadcrumb, breadcrumb-item"));

        if (!showBreadcrumb)
        {
            Assert.Empty(document.QuerySelectorAll("nav.oc-breadcrumb"));
            return;
        }

        var trail = Assert.Single(document.QuerySelectorAll("nav.oc-breadcrumb"));
        Assert.Contains(trailClass, trail.ClassList);
        var nodes = trail.QuerySelectorAll(".breadcrumb-item");
        Assert.Equal(parents.Select(parent => parent.Text).Append(title), nodes.Select(node => node.TextContent));

        for (var i = 0; i < parents.Length; i++)
        {
            Assert.False(nodes[i].HasAttribute("aria-current"));
            if (parents[i].Path is null)
            {
                Assert.Empty(nodes[i].Children);
            }
            else
            {
                var link = Assert.Single(nodes[i].Children);
                Assert.Equal("a", link.LocalName);
                Assert.Empty(link.Children);
                AssertLink(context, link, parents[i].Path);
            }
        }

        var current = Assert.Single(trail.QuerySelectorAll("[aria-current='page']"));
        Assert.Same(nodes[^1], current);
        Assert.Empty(current.Children);
    }

    private static void AssertLink(SiteContext context, IElement link, string path)
    {
        var expected = new Uri($"https://localhost/{context.TenantName}/{path}");
        var href = link.GetAttribute("href");
        Assert.NotNull(href);
        Assert.StartsWith($"/{context.TenantName}/", href);
        var actual = new Uri("https://localhost" + href);
        Assert.Equal(expected.AbsolutePath, actual.AbsolutePath);

        var expectedQuery = QueryHelpers.ParseQuery(expected.Query);
        var actualQuery = QueryHelpers.ParseQuery(actual.Query);
        Assert.Equal(expectedQuery.Count, actualQuery.Count);
        foreach (var (key, value) in expectedQuery)
        {
            Assert.True(actualQuery.TryGetValue(key, out var actualValue));
            Assert.Equal(value, actualValue);
        }
    }
}
