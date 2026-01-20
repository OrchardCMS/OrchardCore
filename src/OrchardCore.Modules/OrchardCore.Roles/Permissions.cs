using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;

namespace OrchardCore.Roles;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageRoles = CommonPermissions.ManageRoles;
    public static readonly Permission AssignRoles = CommonPermissions.AssignRoles;
    public static readonly Permission SiteOwner = StandardPermissions.SiteOwner;

    private readonly IRoleService _roleService;

    public Permissions(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var roleNames = (await _roleService.GetRoleNamesAsync())
            .Where(roleName => !RoleHelper.SystemRoleNames.Contains(roleName))
            .ToList();

        var list = new List<Permission>(roleNames.Count + 3)
        {
            ManageRoles,
            AssignRoles,
            SiteOwner,
        };

        foreach (var roleName in roleNames)
        {
            list.Add(CommonPermissions.CreatePermissionForAssignRole(roleName));
        }

        return list;
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions =
            [
                ManageRoles,
                SiteOwner,
            ],
        },
    ];
}
