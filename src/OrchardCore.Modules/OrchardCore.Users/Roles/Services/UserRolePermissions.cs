using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;

namespace OrchardCore.Users;

public sealed class UserRolePermissions : IPermissionProvider
{
    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Users.CommonPermissions.AssignRoleToUsers'.")]
    public static readonly Permission AssignRoleToUsers = CommonPermissions.AssignRoleToUsers;

    private readonly IRoleService _roleService;

    public UserRolePermissions(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var permissions = new List<Permission>()
        {
            CommonPermissions.AssignRoleToUsers,
        };

        var roleNames = (await _roleService.GetAssignableRolesAsync())
            .Select(role => role.RoleName)
            .OrderBy(roleName => roleName);

        foreach (var roleName in roleNames)
        {
            permissions.Add(CommonPermissions.CreateListUsersInRolePermission(roleName));
            permissions.Add(CommonPermissions.CreateEditUsersInRolePermission(roleName));

            if (!await _roleService.IsAdminRoleAsync(roleName))
            {
                // Do not create permissions for deleting or creating admins.
                // These operations are restricted to admin users only.
                permissions.Add(CommonPermissions.CreateDeleteUsersInRolePermission(roleName));
                permissions.Add(CommonPermissions.CreateAssignRoleToUsersPermission(roleName));
            }

            permissions.Add(CommonPermissions.CreatePermissionForManageUsersInRole(roleName));
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
                CommonPermissions.AssignRoleToUsers,
            ],
        },
    ];
}
