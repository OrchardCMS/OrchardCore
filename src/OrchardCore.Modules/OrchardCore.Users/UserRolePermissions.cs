using System;
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
        var roles = (await _roleService.GetRoleNamesAsync())
            .Except(new[] { "Anonymous", "Authenticated" }, StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x).ToList();

        var list = new List<Permission>();

        foreach (var role in roles)
        {
            list.Add(CommonPermissions.CreateListUsersInRolePermission(role));

            list.Add(CommonPermissions.CreateEditUsersInRolePermission(role));

            list.Add(CommonPermissions.CreateDeleteUsersInRolePermission(role));

            list.Add(CommonPermissions.CreateAssignUsersToRolePermission(role));

            list.Add(CommonPermissions.CreatePermissionForManageUsersInRole(role));
        }

        return list;
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() => Enumerable.Empty<PermissionStereotype>();
}
