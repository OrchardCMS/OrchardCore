using System;
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

        var roles = (await _roleService.GetRoleNamesAsync())
            .Except(_systemRoles, StringComparer.OrdinalIgnoreCase);

        foreach (var role in roles)
        {
            list.Add(CommonPermissions.CreatePermissionForAssignRole(role));
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
            Permissions = _allPermissions,
        },
    ];

    private readonly static IEnumerable<Permission> _allPermissions =
    [
        ManageRoles,
        AssignRoles,
        StandardPermissions.SiteOwner,
    ];

    private static readonly string[] _systemRoles =
    [
        "Anonymous",
        "Authenticated",
    ];
}
