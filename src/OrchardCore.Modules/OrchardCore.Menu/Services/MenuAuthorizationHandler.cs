using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;
using OrchardCore.Contents.Security;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Menu.Services;

/// <summary>
/// Authorization handler that gates access to Menu content items through the ManageMenu permission.
/// <para>
/// By default, the Menu content type is not marked as securable, so generic content permissions
/// like <c>ListContent</c> or <c>EditContent</c> would normally grant access to Menu items
/// through the standard permission hierarchy. This handler overrides that behavior by requiring
/// either the <c>ManageMenu</c> permission or a direct per-type permission claim (e.g.,
/// <c>ListContent_Menu</c>) when the Menu content type is explicitly made securable by an admin.
/// </para>
/// <para>
/// Authorization flow for Menu resources:
/// <list type="bullet">
/// <item>User has <c>ManageMenu</c> → access granted (bridges to all content permissions).</item>
/// <item>User has a direct per-type claim such as <c>EditContent_Menu</c> (from securable) → access granted.</item>
/// <item>User only has generic <c>ListContent</c>/<c>EditContent</c> without <c>ManageMenu</c> → access denied.</item>
/// </list>
/// </para>
/// </summary>
public sealed class MenuAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceProvider _serviceProvider;
    private IAuthorizationService _authorizationService;

    public MenuAuthorizationHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (!IsRelevant(requirement.Permission, context.Resource))
        {
            return;
        }

        // Lazy-resolve to prevent circular dependency (this handler is called by IAuthorizationService).
        _authorizationService ??= _serviceProvider.GetRequiredService<IAuthorizationService>();

        // If the user has ManageMenu (directly or via ImpliedBy chain), grant access.
        if (await _authorizationService.AuthorizeAsync(context.User, Permissions.ManageMenu))
        {
            context.Succeed(requirement);
            return;
        }

        // If the Menu content type has been explicitly made securable and the user's role has
        // been granted the specific per-type permission (e.g., ListContent_Menu), allow access.
        // We check claims directly to avoid matching generic permissions like ListContent which
        // would otherwise imply ListContent_Menu through the standard permission hierarchy.
        if (context.User.HasClaim(Permission.ClaimType, requirement.Permission.Name))
        {
            context.Succeed(requirement);
            return;
        }

        // Deny access: the user has neither ManageMenu nor a direct per-type permission claim.
        // This prevents generic content permissions (ListContent, EditContent, etc.) from
        // granting implicit access to Menu content items.
        context.Fail(new AuthorizationFailureReason(this, "ManageMenu permission is required to access Menu content items."));
    }

    private static bool IsRelevant(Permission permission, object resource)
        => IsContentPermission(permission) && IsMenuResource(resource);

    private static bool IsContentPermission(Permission permission)
    {
        // Match both the dynamic content-type permissions generated for Menu (for example Edit_Menu)
        // and the generic content permissions that are evaluated before Orchard maps them per content type.
        if (ContentTypePermissionsHelper.PermissionTemplates.Values.Any(template =>
            string.Equals(permission.Name, string.Format(template.Name, MenuConstants.MenuContentType), StringComparison.Ordinal)))
        {
            return true;
        }

        return permission.Name switch
        {
            nameof(CommonPermissions.ListContent) => true,
            nameof(CommonPermissions.PublishContent) => true,
            nameof(CommonPermissions.PublishOwnContent) => true,
            nameof(CommonPermissions.EditContent) => true,
            nameof(CommonPermissions.EditOwnContent) => true,
            nameof(CommonPermissions.DeleteContent) => true,
            nameof(CommonPermissions.DeleteOwnContent) => true,
            nameof(CommonPermissions.ViewContent) => true,
            nameof(CommonPermissions.ViewOwnContent) => true,
            nameof(CommonPermissions.PreviewContent) => true,
            nameof(CommonPermissions.PreviewOwnContent) => true,
            nameof(CommonPermissions.CloneContent) => true,
            nameof(CommonPermissions.CloneOwnContent) => true,
            nameof(CommonPermissions.EditContentOwner) => true,
            _ => false,
        };
    }

    private static bool IsMenuResource(object resource)
    {
        // Content authorization can be evaluated against either a content item instance or the content type name.
        if (resource is ContentItem contentItem)
        {
            return string.Equals(contentItem.ContentType, MenuConstants.MenuContentType, StringComparison.Ordinal);
        }

        if (resource is string contentType)
        {
            return string.Equals(contentType, MenuConstants.MenuContentType, StringComparison.Ordinal);
        }

        return false;
    }
}
