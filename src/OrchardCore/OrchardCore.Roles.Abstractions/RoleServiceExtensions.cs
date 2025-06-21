using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Security.Services;

public static class RoleServiceExtensions
{
    public static async Task<IEnumerable<string>> GetRoleNamesAsync(this IRoleService roleService)
    {
        var roles = await roleService.GetRolesAsync().ConfigureAwait(false);

        return roles.Select(r => r.RoleName);
    }

    public static async Task<IEnumerable<IRole>> GetAssignableRolesAsync(this IRoleService roleService)
    {
        var roles = await roleService.GetRolesAsync().ConfigureAwait(false);

        var assignableRoles = new List<IRole>();
        foreach (var role in roles)
        {
            if (!await roleService.IsAdminRoleAsync(role.RoleName).ConfigureAwait(false) && await roleService.IsSystemRoleAsync(role.RoleName).ConfigureAwait(false))
            {
                continue;
            }

            assignableRoles.Add(role);
        }

        return assignableRoles;
    }

    public static async Task<IEnumerable<IRole>> GetAccessibleRolesAsync(this IRoleService roleService, IAuthorizationService authorizationService, ClaimsPrincipal user, Permission permission)
    {
        var roles = await roleService.GetAssignableRolesAsync().ConfigureAwait(false);

        var accessibleRoles = new List<IRole>();

        foreach (var role in roles)
        {
            if (!await authorizationService.AuthorizeAsync(user, permission, role).ConfigureAwait(false))
            {
                continue;
            }

            accessibleRoles.Add(role);
        }

        return accessibleRoles;
    }
}
