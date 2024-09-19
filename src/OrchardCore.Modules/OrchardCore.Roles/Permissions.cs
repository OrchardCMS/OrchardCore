using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;

namespace OrchardCore.Roles;

public sealed class Permissions : IPermissionProvider
{
    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Roles.CommonPermissions.ManageRoles'.")]
    public static readonly Permission ManageRoles = CommonPermissions.ManageRoles;

    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Roles.CommonPermissions.AssignRoles'.")]
    public static readonly Permission AssignRoles = CommonPermissions.AssignRoles;

    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Security.StandardPermissions.SiteOwner'.")]
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
            .ToArray();

        var list = new List<Permission>(roleNames.Length + 2)
        {
            CommonPermissions.ManageRoles,
            StandardPermissions.SiteOwner,
        };

        return list;
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions =
            [
                CommonPermissions.ManageRoles,
                StandardPermissions.SiteOwner,
            ],
        },
    ];
}
