using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents;
using OrchardCore.Dashboards.Models;
using OrchardCore.Dashboards.Services;
using OrchardCore.Security;

namespace OrchardCore.Dashboards.Security;

/// <summary>
/// Lets users build their own <see cref="DashboardScope.User"/> dashboards without being granted content permissions
/// on the widget types: it grants the content permissions on the widgets a user owns on such a dashboard, and the
/// permission to create a widget, when the user can manage one of these dashboards.
/// </summary>
/// <remarks>
/// The dashboard a new widget is added to is validated by the <see cref="DashboardPart"/> editor, which rejects
/// a dashboard the user can't manage.
/// </remarks>
internal sealed class UserDashboardWidgetAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private static readonly HashSet<string> s_ownWidgetPermissions =
    [
        CommonPermissions.ViewContent.Name,
        CommonPermissions.ViewOwnContent.Name,
        CommonPermissions.PreviewContent.Name,
        CommonPermissions.PreviewOwnContent.Name,
        CommonPermissions.EditContent.Name,
        CommonPermissions.EditOwnContent.Name,
        CommonPermissions.PublishContent.Name,
        CommonPermissions.PublishOwnContent.Name,
        CommonPermissions.DeleteContent.Name,
        CommonPermissions.DeleteOwnContent.Name,
    ];

    private static readonly HashSet<string> s_newWidgetPermissions =
    [
        CommonPermissions.EditContent.Name,
        CommonPermissions.EditOwnContent.Name,
        CommonPermissions.PublishContent.Name,
        CommonPermissions.PublishOwnContent.Name,
    ];

    private readonly IServiceProvider _serviceProvider;

    public UserDashboardWidgetAuthorizationHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.HasSucceeded ||
            context.Resource is not ContentItem contentItem ||
            requirement.Permission is null ||
            !s_ownWidgetPermissions.Contains(requirement.Permission.Name))
        {
            return;
        }

        var userId = DashboardManager.GetUserId(context.User);

        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        // Lazy load to prevent circular dependencies with the authorization service.
        var dashboardManager = _serviceProvider.GetRequiredService<IDashboardManager>();

        if (contentItem.TryGet<DashboardPart>(out var part) && !string.IsNullOrEmpty(part.DashboardType))
        {
            // A widget of a dashboard: granted to its owner on a dashboard of their own.
            var type = dashboardManager.GetType(part.DashboardType);

            if (type?.Scope == DashboardScope.User &&
                dashboardManager.IsOnDashboard(context.User, type, contentItem) &&
                await dashboardManager.CanManageAsync(context.User, type))
            {
                context.Succeed(requirement);
            }

            return;
        }

        // A widget not saved yet, or a check on its content type: granted to create a widget of a dashboard of their own.
        if (contentItem.Id != 0 ||
            !s_newWidgetPermissions.Contains(requirement.Permission.Name) ||
            !await IsWidgetTypeAsync(contentItem.ContentType))
        {
            return;
        }

        foreach (var type in dashboardManager.GetTypes())
        {
            if (type.Scope == DashboardScope.User && await dashboardManager.CanManageAsync(context.User, type))
            {
                context.Succeed(requirement);

                return;
            }
        }
    }

    private async Task<bool> IsWidgetTypeAsync(string contentType)
    {
        if (string.IsNullOrEmpty(contentType))
        {
            return false;
        }

        var contentDefinitionManager = _serviceProvider.GetRequiredService<IContentDefinitionManager>();
        var typeDefinition = await contentDefinitionManager.GetTypeDefinitionAsync(contentType);

        return typeDefinition?.StereotypeEquals(DashboardConstants.WidgetStereotype) == true;
    }
}
