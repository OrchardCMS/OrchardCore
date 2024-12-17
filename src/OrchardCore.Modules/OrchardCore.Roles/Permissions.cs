using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Roles;

public sealed class Permissions : IPermissionProvider
{
    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Roles.CommonPermissions.ManageRoles'.")]
    public static readonly Permission ManageRoles = CommonPermissions.ManageRoles;

    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Roles.CommonPermissions.AssignRoles'.")]
    public static readonly Permission AssignRoles = CommonPermissions.AssignRoles;

    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Security.StandardPermissions.SiteOwner'.")]
    public static readonly Permission SiteOwner = StandardPermissions.SiteOwner;

    private readonly IEnumerable<Permission> _allPermissions =
    [
        CommonPermissions.ManageRoles,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = _allPermissions,
        },
    ];
}
