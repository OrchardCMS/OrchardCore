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

    private readonly IRoleService _roleService;

    public Permissions(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var list = new List<Permission>(_allPermissions);

        var roleNames = (await _roleService.GetRoleNamesAsync())
            .Where(roleName => !RoleHelper.SystemRoleNames.Contains(roleName));

        foreach (var roleName in roleNames)
        {
            list.Add(CommonPermissions.CreatePermissionForAssignRole(roleName));
        }

        return list;
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _allStereotypes;

    private readonly static IEnumerable<PermissionStereotype> _allStereotypes =
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions =
            [
                ManageRoles,
                StandardPermissions.SiteOwner,
            ],
        },
    ];

    private readonly static IEnumerable<Permission> _allPermissions =
    [
        ManageRoles,
        AssignRoles,
        StandardPermissions.SiteOwner,
    ];
}
