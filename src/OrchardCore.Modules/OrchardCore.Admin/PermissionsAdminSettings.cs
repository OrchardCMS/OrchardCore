using OrchardCore.Security.Permissions;

namespace OrchardCore.Admin;

public sealed class PermissionsAdminSettings : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        AdminPermissions.ManageAdminSettings,
    ];

    [Obsolete("This will be removed in a future release. Instead use 'AdminPermissions.ManageAdminSettings'.")]
    public static readonly Permission ManageAdminSettings = AdminPermissions.ManageAdminSettings;

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = _allPermissions,
        },
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);
}
