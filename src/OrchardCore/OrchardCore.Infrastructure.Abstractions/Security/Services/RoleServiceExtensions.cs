using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Security.Services;

public static class RoleServiceExtensions
{
    public static async Task<IEnumerable<string>> GetRoleNamesAsync(this IRoleService roleService)
    {
        var roles = await roleService.GetRolesAsync();

        return roles.Select(r => r.RoleName);
    }

    public static async Task<IEnumerable<IRole>> GetAssignableRolesAsync(this IRoleService roleService)
    {
        var roles = await roleService.GetRolesAsync();

        var assignableRoles = new List<IRole>();
        foreach (var role in roles)
        {
            if (!await roleService.IsAdminRoleAsync(role.RoleName) && await roleService.IsSystemRoleAsync(role.RoleName))
            {
                continue;
            }

            assignableRoles.Add(role);
        }

        return assignableRoles;
    }

    public static async Task<IEnumerable<IRole>> GetAccessibleRolesAsync(this IRoleService roleService, IAuthorizationService authorizationService, ClaimsPrincipal user, Permission permission)
    {
        var roles = await roleService.GetAssignableRolesAsync();

        var accessibleRoles = new List<IRole>();

        foreach (var role in roles)
        {
            if (!await authorizationService.AuthorizeAsync(user, permission, role))
            {
                continue;
            }

            accessibleRoles.Add(role);
        }

        return accessibleRoles;
    }

    [Obsolete("This method is obsolete and will be removed in future releases.")]
    public static async Task<IEnumerable<string>> GetAccessibleRoleNamesAsync(this IRoleService roleService, IAuthorizationService authorizationService, ClaimsPrincipal user, Permission permission)
    {
        var roles = await roleService.GetAccessibleRolesAsync(authorizationService, user, permission);

        return roles.Select(x => x.RoleName);
    }
}
