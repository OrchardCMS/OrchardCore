using OrchardCore.Environment.Shell;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;

namespace OrchardCore.Users;

public sealed class UserRolePermissions : IPermissionProvider
{
    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Users.CommonPermissions.AssignRoleToUsers'.")]
    public static readonly Permission AssignRoleToUsers = CommonPermissions.AssignRoleToUsers;

    private readonly IRoleService _roleService;
    private readonly ShellSettings _shellSettings;

    public UserRolePermissions(
        IRoleService roleService,
        ShellSettings shellSettings)
    {
        _roleService = roleService;
        _shellSettings = shellSettings;
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

        var adminRoleName = _shellSettings.GetSystemAdminRoleName();

        foreach (var roleName in roleNames)
        {
            permissions.Add(CommonPermissions.CreateListUsersInRolePermission(roleName));
            permissions.Add(CommonPermissions.CreateEditUsersInRolePermission(roleName));
            permissions.Add(CommonPermissions.CreateDeleteUsersInRolePermission(roleName));

            if (!roleName.Equals(adminRoleName, StringComparison.OrdinalIgnoreCase))
            {
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
