using OrchardCore.Security.Permissions;

namespace OrchardCore.Settings;

public sealed class Permissions : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        SettingsPermissions.ManageSettings,
    ];

    [Obsolete("This will be removed in a future release. Instead use 'SettingsPermissions.ManageSettings'.")]
    public static readonly Permission ManageSettings = SettingsPermissions.ManageSettings;

    [Obsolete("This will be removed in a future release. Instead use 'SettingsPermissions.ManageGroupSettings'.")]
    public static readonly Permission ManageGroupSettings = SettingsPermissions.ManageGroupSettings;

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
