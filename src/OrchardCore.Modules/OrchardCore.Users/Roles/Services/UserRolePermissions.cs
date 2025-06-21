using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;

namespace OrchardCore.Users;

public sealed class UserRolePermissions : IPermissionProvider
{
    private readonly IRoleService _roleService;

    public UserRolePermissions(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var permissions = new List<Permission>()
        {
            UsersPermissions.AssignRoleToUsers,
        };

        var roleNames = (await _roleService.GetAssignableRolesAsync().ConfigureAwait(false))
            .Select(role => role.RoleName)
            .OrderBy(roleName => roleName);

        foreach (var roleName in roleNames)
        {
            permissions.Add(UsersPermissions.CreateListUsersInRolePermission(roleName));
            permissions.Add(UsersPermissions.CreateEditUsersInRolePermission(roleName));

            if (!await _roleService.IsAdminRoleAsync(roleName).ConfigureAwait(false))
            {
                // Do not create permissions for deleting or creating admins.
                // These operations are restricted to admin users only.
                permissions.Add(UsersPermissions.CreateDeleteUsersInRolePermission(roleName));
                permissions.Add(UsersPermissions.CreateAssignRoleToUsersPermission(roleName));
            }

            permissions.Add(UsersPermissions.CreatePermissionForManageUsersInRole(roleName));
        }

        return permissions;
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions =
            [
                UsersPermissions.AssignRoleToUsers,
            ],
        },
    ];
}
