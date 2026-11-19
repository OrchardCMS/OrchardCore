using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Dashboards;
using OrchardCore.Dashboards.Models;
using OrchardCore.Dashboards.Services;
using OrchardCore.Localization;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Tests.Modules.OrchardCore.Dashboards;

internal static class DashboardTestHelper
{
    public const string WidgetType = "HtmlDashboardWidget";
    public const string SiteType = "Site";
    public const string UserType = "Mine";

    public static readonly Permission ViewSite = new("ViewSiteDashboard", LocalizationSource.Create("View site dashboard"));
    public static readonly Permission ManageSite = new("ManageSiteDashboard", LocalizationSource.Create("Manage site dashboard"));
    public static readonly Permission ManageOwn = new("ManageOwnTestDashboard", LocalizationSource.Create("Manage own dashboard"));

    public static DashboardOptions CreateOptions()
    {
        var options = new DashboardOptions { DefaultType = SiteType };

        options.Add(SiteType, LocalizationSource.Create("Site"), type =>
        {
            type.ViewPermission = ViewSite;
            type.ManagePermission = ManageSite;
        });

        options.Add(UserType, LocalizationSource.Create("Mine"), type =>
        {
            type.Scope = DashboardScope.User;
            type.ManagePermission = ManageOwn;
        });

        return options;
    }

    public static DashboardManager CreateManager()
        => new(Mock.Of<YesSql.ISession>(), new PermissionClaimsAuthorizationService(), Options.Create(CreateOptions()));

    public static ClaimsPrincipal CreateUser(string userId, params Permission[] permissions)
    {
        var identity = new ClaimsIdentity("Test");

        if (userId is not null)
        {
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
        }

        foreach (var permission in permissions)
        {
            identity.AddClaim(new Claim(Permission.ClaimType, permission.Name));
        }

        return new ClaimsPrincipal(identity);
    }

    public static ContentItem CreateWidget(string dashboardType, string ownerId, long id = 1)
    {
        var widget = new ContentItem
        {
            Id = id,
            ContentItemId = "widget" + id,
            ContentType = WidgetType,
        };

        widget.Apply(new DashboardPart
        {
            DashboardType = dashboardType,
            OwnerId = ownerId,
        });

        return widget;
    }

    public static IContentDefinitionManager CreateContentDefinitionManager()
    {
        var settings = new JsonObject
        {
            ["ContentTypeSettings"] = new JsonObject
            {
                ["Stereotype"] = DashboardConstants.WidgetStereotype,
            },
        };

        var widgetDefinition = new ContentTypeDefinition(WidgetType, WidgetType, [], settings);
        var articleDefinition = new ContentTypeDefinition("Article", "Article", [], []);

        var manager = new Mock<IContentDefinitionManager>();
        manager.Setup(m => m.GetTypeDefinitionAsync(WidgetType)).ReturnsAsync(widgetDefinition);
        manager.Setup(m => m.GetTypeDefinitionAsync("Article")).ReturnsAsync(articleDefinition);

        return manager.Object;
    }

    /// <summary>
    /// Grants the permissions the user has a claim for, ignoring the resource.
    /// </summary>
    private sealed class PermissionClaimsAuthorizationService : IAuthorizationService
    {
        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object resource, IEnumerable<IAuthorizationRequirement> requirements)
        {
            var granted = requirements.All(requirement =>
                requirement is PermissionRequirement permissionRequirement &&
                user.HasClaim(Permission.ClaimType, permissionRequirement.Permission.Name));

            return Task.FromResult(granted ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        }

        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object resource, string policyName)
            => Task.FromResult(AuthorizationResult.Failed());
    }
}
