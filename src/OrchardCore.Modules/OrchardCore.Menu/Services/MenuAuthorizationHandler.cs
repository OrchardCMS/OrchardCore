using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;
using OrchardCore.Contents.Security;
using OrchardCore.Roles;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Menu.Services;

/// <summary>
/// Aligns Menu content authorization with the Menu-specific permission graph.
/// </summary>
public sealed class MenuAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private static readonly Permission s_editMenuContent = ContentTypePermissionsHelper.CreateDynamicPermission(
        ContentTypePermissionsHelper.PermissionTemplates[CommonPermissions.EditContent.Name],
        MenuConstants.MenuContentType);
    private static readonly Permission s_manageMenuPermission = new(Permissions.ManageMenu.Name, Permissions.ManageMenu.Description);

    private readonly IServiceProvider _serviceProvider;
    private readonly ISystemRoleProvider _systemRoleProvider;
    private readonly ISiteService _siteService;
    private IAuthorizationService _authorizationService;

    public MenuAuthorizationHandler(IServiceProvider serviceProvider, ISystemRoleProvider systemRoleProvider, ISiteService siteService)
    {
        _serviceProvider = serviceProvider;
        _systemRoleProvider = systemRoleProvider;
        _siteService = siteService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.HasSucceeded || !IsMenuResource(context.Resource))
        {
            return;
        }

        var menuPermission = GetMenuPermission(context.User, requirement.Permission, context.Resource);

        if (menuPermission is null)
        {
            return;
        }

        if (await IsAdministratorOrSuperUserAsync(context.User))
        {
            return;
        }

        // Lazy-resolve to prevent circular dependency (this handler is called by IAuthorizationService).
        _authorizationService ??= _serviceProvider.GetRequiredService<IAuthorizationService>();

        var hasDirectMenuPermission = HasDirectPermission(context.User, menuPermission);
        var hasManageMenuPermission = HasDirectPermission(context.User, s_manageMenuPermission)
            || await _authorizationService.AuthorizeAsync(context.User, s_manageMenuPermission);

        if (hasDirectMenuPermission
            || await _authorizationService.AuthorizeAsync(context.User, new Permission(menuPermission.Name))
            || IsListPermission(menuPermission) && hasManageMenuPermission
            || hasManageMenuPermission && IsGrantedByEditMenu(menuPermission))
        {
            context.Succeed(requirement);
            return;
        }

        context.Fail(new AuthorizationFailureReason(this, "The requested Menu operation is not allowed."));
    }

    private static Permission GetMenuPermission(ClaimsPrincipal user, Permission permission, object resource)
    {
        if (permission is null)
        {
            return null;
        }

        if (resource is ContentItem contentItem
            && CommonPermissions.OwnerPermissionsByName.TryGetValue(permission.Name, out var ownerPermission)
            && HasOwnership(user, contentItem))
        {
            permission = ownerPermission;
        }

        if (IsMenuPermission(permission))
        {
            return permission;
        }

        var permissionTemplate = ContentTypePermissionsHelper.ConvertToDynamicPermission(permission);

        return permissionTemplate is null
            ? null
            : ContentTypePermissionsHelper.CreateDynamicPermission(permissionTemplate, MenuConstants.MenuContentType);
    }

    private static bool IsGrantedByEditMenu(Permission permission)
    {
        var grantingNames = new HashSet<string>(StringComparer.Ordinal);

        AddMenuGrantingNames(permission, grantingNames);

        return grantingNames.Contains(s_editMenuContent.Name);
    }

    private static void AddMenuGrantingNames(Permission permission, HashSet<string> grantingNames)
    {
        if (!grantingNames.Add(permission.Name) || permission.ImpliedBy is null)
        {
            return;
        }

        foreach (var impliedBy in permission.ImpliedBy)
        {
            if (!IsMenuPermission(impliedBy))
            {
                continue;
            }

            AddMenuGrantingNames(impliedBy, grantingNames);
        }
    }

    private static bool HasDirectPermission(ClaimsPrincipal user, Permission permission)
        => user.HasClaim(Permission.ClaimType, permission.Name);

    private async Task<bool> IsAdministratorOrSuperUserAsync(ClaimsPrincipal user)
    {
        if (user is null)
        {
            return false;
        }

        var adminRole = _systemRoleProvider.GetAdminRole();

        if (adminRole is not null && user.IsInRole(adminRole.RoleName))
        {
            return true;
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        var site = await _siteService.GetSiteSettingsAsync();

        return string.Equals(userId, site.SuperUser, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsListPermission(Permission permission)
        => string.Equals(permission.Name, Permissions.ListMenuContent.Name, StringComparison.Ordinal);

    private static bool IsMenuPermission(Permission permission)
    {
        if (permission is null)
        {
            return false;
        }

        return ContentTypePermissionsHelper.PermissionTemplates.Values.Any(template =>
            string.Equals(permission.Name, string.Format(template.Name, MenuConstants.MenuContentType), StringComparison.Ordinal));
    }

    private static bool IsMenuResource(object resource)
    {
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

    private static bool HasOwnership(ClaimsPrincipal user, ContentItem content)
    {
        if (user == null || content == null)
        {
            return false;
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(content.Owner))
        {
            return false;
        }

        return userId == content.Owner;
    }
}
