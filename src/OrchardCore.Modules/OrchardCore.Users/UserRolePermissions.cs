using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;

namespace OrchardCore.Users;

public class UserRolePermissions : IPermissionProvider
{
    private readonly IRoleService _roleService;

    public UserRolePermissions(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var roleNames = (await _roleService.GetRoleNamesAsync())
            .Where(roleName => !RoleHelper.SystemRoleNames.Contains(roleName))
            .OrderBy(roleName => roleName);

        var list = new List<Permission>()
        {
            CommonPermissions.AssignRoleToUsers,
        };

        foreach (var roleName in roleNames)
        {
            list.Add(CommonPermissions.CreateListUsersInRolePermission(roleName));
            list.Add(CommonPermissions.CreateEditUsersInRolePermission(roleName));
            list.Add(CommonPermissions.CreateDeleteUsersInRolePermission(roleName));
            list.Add(CommonPermissions.CreateAssignRoleToUsersPermission(roleName));
            list.Add(CommonPermissions.CreatePermissionForManageUsersInRole(roleName));
        }

        return list;
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
    {
        return new[] {
            new PermissionStereotype
            {
                Name = "Administrator",
                Permissions = new[] {
                    CommonPermissions.AssignRoleToUsers,
                }
            }
        };
    }
}
