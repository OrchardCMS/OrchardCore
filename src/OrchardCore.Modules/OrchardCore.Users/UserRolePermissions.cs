using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;

namespace OrchardCore.Users;

public class UserRolePermissions : IPermissionProvider
{
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
            AssignRoleToUsers,
        };

        var roleNames = (await _roleService.GetRoleNamesAsync())
        .Where(roleName => !RoleHelper.SystemRoleNames.Contains(roleName))
        .OrderBy(roleName => roleName);

        foreach (var roleName in roleNames)
        {
            permissions.Add(CommonPermissions.CreateListUsersInRolePermission(roleName));
            permissions.Add(CommonPermissions.CreateEditUsersInRolePermission(roleName));
            permissions.Add(CommonPermissions.CreateDeleteUsersInRolePermission(roleName));
            permissions.Add(CommonPermissions.CreateAssignRoleToUsersPermission(roleName));
            permissions.Add(CommonPermissions.CreatePermissionForManageUsersInRole(roleName));
        }

        return permissions;
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions =
            [
                AssignRoleToUsers,
            ],
        },
    ];
}
