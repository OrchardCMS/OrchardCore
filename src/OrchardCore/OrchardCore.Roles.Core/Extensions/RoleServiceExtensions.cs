using OrchardCore.Security.Services;

namespace OrchardCore.Roles.Extensions;

public static class RoleServiceExtensions
{
    public static async Task<IEnumerable<string>> GetRoleNamesAsync(this IRoleService roleService)
    {
        ArgumentNullException.ThrowIfNull(roleService);

        var roles = await roleService.GetRolesAsync();

        return roles.Select(role => role.RoleName);
    }
}
