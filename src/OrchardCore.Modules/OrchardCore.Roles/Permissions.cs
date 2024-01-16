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

    private static readonly IEnumerable<PermissionStereotype> _stereotypes =
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

    private readonly IRoleService _roleService;

    public Permissions(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var list = new List<Permission>()
        {
            ManageRoles,
            AssignRoles,
            SiteOwner,
        };

        var roleNames = (await _roleService.GetRoleNamesAsync())
            .Where(roleName => !RoleHelper.SystemRoleNames.Contains(roleName));

        foreach (var roleName in roleNames)
        {
            list.Add(CommonPermissions.CreatePermissionForAssignRole(roleName));
        }

        return list;
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _stereotypes;
}
