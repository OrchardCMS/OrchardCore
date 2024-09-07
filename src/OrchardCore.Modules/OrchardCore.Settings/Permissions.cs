using OrchardCore.Security.Permissions;

namespace OrchardCore.Settings;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageSettings = new("ManageSettings", "Manage settings");

    // This permission is not exposed, it's just used for the APIs to generate/check custom ones.
    public static readonly Permission ManageGroupSettings = new("ManageResourceSettings", "Manage settings", new[] { ManageSettings });

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageSettings,
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
