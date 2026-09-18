using System.Text.Json.Nodes;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Templates;
using OrchardCore.Templates.Models;
using OrchardCore.Templates.Services;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.UrlRewriting;
using OrchardCore.UrlRewriting.Services;
using TemplatePermissions = OrchardCore.Templates.Permissions;

namespace OrchardCore.Tests.Modules;

public class ManagementModulesInlineBreadcrumbIntegrationTests
{
    [Theory]
    [InlineData("OrchardCore.AuditTrail", "Admin/AuditTrail", "breadcrumb-audit-trail", "Audit Trail", null, null)]
    [InlineData("OrchardCore.BackgroundTasks", "Admin/BackgroundTasks", "breadcrumb-background-tasks", "Background Tasks", null, null)]
    [InlineData("OrchardCore.DataLocalization", "Admin/DataLocalization/Statistics", "breadcrumb-dynamic-translations-statistics", "Statistics", "Dynamic Translations", "Admin/DataLocalization/Index")]
    [InlineData("OrchardCore.Media", "Admin/MediaProfiles/Create", "breadcrumb-media-profiles-create", "Create Media Profile", "Media Profiles", "Admin/MediaProfiles")]
    [InlineData("OrchardCore.OpenId.Management", "Admin/OpenId/Scope/Create", "breadcrumb-open-id-scopes-create", "Create a new scope", "Scopes", "Admin/OpenId/Scope")]
    [InlineData("OrchardCore.Placements", "Admin/Placements/Create", "breadcrumb-placements-edit", "Create Placement", "Placements", "Admin/Placements")]
    [InlineData("OrchardCore.Placements", "Admin/Placements/Edit/BreadcrumbTest", "breadcrumb-placements-edit", "Edit Placement", "Placements", "Admin/Placements")]
    [InlineData("OrchardCore.Users", "Admin/Users/Create", "breadcrumb-users-create", "Create User", "Users", "Admin/Users/Index")]
    public async Task Render_ConvertedPage_PreservesTrailAndTenantRelativeParent(
        string feature,
        string path,
        string trailClass,
        string title,
        string parentText,
        string parentPath)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await ConfigureAsync(context, showBreadcrumb: true, feature);

        using var document = new HtmlParser().ParseDocument(await GetPageAsync(context, path));

        AssertBreadcrumb(context, document, trailClass, title, showBreadcrumb: true, parentText, parentPath);
    }

    [Theory]
    [InlineData("Edit", true)]
    [InlineData("Clone", true)]
    [InlineData("Display", true)]
    [InlineData("Edit", false)]
    [InlineData("Clone", false)]
    [InlineData("Display", false)]
    public async Task Roles_DynamicName_EncodesLabelsOnceAndPreservesParent(string action, bool showBreadcrumb)
    {
        const string roleName = "Editors & <breadcrumb-test> \"team\" 'owners' &amp;";
        using var context = new SiteContext();
        await context.InitializeAsync();
        await ConfigureAsync(context, showBreadcrumb, "OrchardCore.Roles");

        await context.UsingTenantScopeAsync(async scope =>
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IRole>>();
            var result = await roleManager.CreateAsync(new Role { RoleName = roleName });
            Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(error => error.Description)));
        });

        var title = action == "Display" ? roleName : $"{action} '{roleName}' Role";
        var path = $"Admin/Roles/{action}/{Uri.EscapeDataString(roleName)}";
        using var document = new HtmlParser().ParseDocument(await GetPageAsync(context, path));

        AssertBreadcrumb(context, document, $"breadcrumb-roles-{action.ToLowerInvariant()}", title, showBreadcrumb, "Roles", "Admin/Roles/Index");
        Assert.Empty(document.QuerySelectorAll("breadcrumb-test"));
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(true, false)]
    public async Task Templates_AdminVariant_PreservesListActionAndTitles(bool adminTemplates, bool showBreadcrumb)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await ConfigureAsync(context, showBreadcrumb, "OrchardCore.AdminTemplates");

        await context.UsingTenantScopeAsync(async scope =>
        {
            var template = new Template { Content = "Breadcrumb test" };
            if (adminTemplates)
            {
                await scope.ServiceProvider.GetRequiredService<AdminTemplatesManager>()
                    .UpdateTemplateAsync("BreadcrumbTest", template);
            }
            else
            {
                await scope.ServiceProvider.GetRequiredService<TemplatesManager>()
                    .UpdateTemplateAsync("BreadcrumbTest", template);
            }
        });

        var parentText = adminTemplates ? "Admin Templates" : "Templates";
        var parentPath = adminTemplates ? "Admin/Templates/Admin" : "Admin/Templates";
        var query = $"?adminTemplates={adminTemplates}";

        using var list = new HtmlParser().ParseDocument(await GetPageAsync(context, parentPath));
        AssertBreadcrumb(context, list, "breadcrumb-templates", parentText, showBreadcrumb);

        using var create = new HtmlParser().ParseDocument(await GetPageAsync(context, "Admin/Templates/Create" + query));
        AssertBreadcrumb(context, create, "breadcrumb-templates-create", "Create Template", showBreadcrumb, parentText, parentPath);

        using var edit = new HtmlParser().ParseDocument(await GetPageAsync(context, "Admin/Templates/Edit/BreadcrumbTest" + query));
        AssertBreadcrumb(context, edit, "breadcrumb-templates-edit", adminTemplates ? "Edit Admin Template" : "Edit Template", showBreadcrumb, parentText, parentPath);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Templates_ParentPermission_ControlsLinkWithoutRemovingLabel(bool canManageTemplates)
    {
        List<Permission> permissions =
        [
            AdminPermissions.AccessAdminPanel,
            AdminTemplatesPermissions.ManageAdminTemplates,
        ];
        if (canManageTemplates)
        {
            permissions.Add(TemplatePermissions.ManageTemplates);
        }

        using var context = new SiteContext().WithPermissionsContext(new PermissionsContext
        {
            UsePermissionsContext = true,
            AuthorizedPermissions = permissions,
        });
        await context.InitializeAsync();
        await ConfigureAsync(context, showBreadcrumb: true, "OrchardCore.AdminTemplates");

        using var document = new HtmlParser().ParseDocument(
            await GetPageAsync(context, "Admin/Templates/Create?adminTemplates=true"));

        AssertBreadcrumb(
            context,
            document,
            "breadcrumb-templates-create",
            "Create Template",
            showBreadcrumb: true,
            "Admin Templates",
            "Admin/Templates/Admin",
            parentLinkEnabled: canManageTemplates);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UrlRewriting_RuleName_EncodesLabelsOnceAndPreservesParent(bool showBreadcrumb)
    {
        const string ruleName = "Redirect & <breadcrumb-test> \"links\" 'today' &amp;";
        using var context = new SiteContext();
        await context.InitializeAsync();
        await ConfigureAsync(context, showBreadcrumb, "OrchardCore.UrlRewriting");

        string ruleId = null;
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IRewriteRulesManager>();
            var rule = await manager.NewAsync(UrlRedirectRuleSource.SourceName, new JsonObject
            {
                ["Pattern"] = "^breadcrumb-source$",
                ["SubstitutionPattern"] = "/breadcrumb-target",
            });
            Assert.NotNull(rule);
            rule.Name = ruleName;
            await manager.SaveAsync(rule);
            ruleId = rule.Id;
        });
        await context.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);

        using var document = new HtmlParser().ParseDocument(
            await GetPageAsync(context, $"Admin/UrlRewriting/Edit/{ruleId}"));

        AssertBreadcrumb(context, document, "breadcrumb-url-rewriting-edit", $"Edit '{ruleName}' rule", showBreadcrumb, "URL Rewriting Rules", "Admin/UrlRewriting/Index");
        Assert.Empty(document.QuerySelectorAll("breadcrumb-test"));
    }

    private static async Task ConfigureAsync(SiteContext context, bool showBreadcrumb, params string[] featureIds)
    {
        await context.UsingTenantScopeAsync(async scope =>
        {
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
            var site = await siteService.LoadSiteSettingsAsync();
            site.Put(new AdminSettings { ShowBreadcrumb = showBreadcrumb });
            await siteService.UpdateSiteSettingsAsync(site);

            var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var availableFeatures = (await featuresManager.GetAvailableFeaturesAsync()).ToArray();
            var features = featureIds.Select(id => Assert.Single(availableFeatures, feature => feature.Id == id)).ToArray();
            await featuresManager.EnableFeaturesAsync(features, force: true);
        });

        await context.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);
    }

    private static async Task<string> GetPageAsync(SiteContext context, string path)
    {
        using var response = await context.Client.GetAsync(path, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    }

    private static void AssertBreadcrumb(
        SiteContext context,
        IDocument document,
        string trailClass,
        string title,
        bool showBreadcrumb,
        string parentText = null,
        string parentPath = null,
        bool parentLinkEnabled = true)
    {
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
        var items = trail.QuerySelectorAll(".breadcrumb-item");
        string[] expected = parentText == null ? [title] : [parentText, title];
        Assert.Equal(expected, items.Select(item => item.TextContent));

        if (parentText != null)
        {
            if (parentLinkEnabled)
            {
                var link = Assert.Single(items[0].QuerySelectorAll("a"));
                Assert.Equal($"/{context.TenantName}/{parentPath}", link.GetAttribute("href"));
            }
            else
            {
                Assert.Empty(items[0].QuerySelectorAll("a"));
            }
        }

        var current = Assert.Single(trail.QuerySelectorAll("[aria-current='page']"));
        Assert.Equal(title, current.TextContent);
        Assert.Empty(current.QuerySelectorAll("a"));
    }
}
