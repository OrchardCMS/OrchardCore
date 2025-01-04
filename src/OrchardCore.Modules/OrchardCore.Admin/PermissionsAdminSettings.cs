using OrchardCore.Security.Permissions;

namespace OrchardCore.Admin;

public sealed class PermissionsAdminSettings : IPermissionProvider
{
    public static readonly Permission ManageAdminSettings = new("ManageAdminSettings", "Manage Admin Settings");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageAdminSettings,
    ];

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
