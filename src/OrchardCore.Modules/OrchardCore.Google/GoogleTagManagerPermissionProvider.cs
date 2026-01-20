using OrchardCore.Security.Permissions;

namespace OrchardCore.Google;

public sealed class GoogleTagManagerPermissionProvider : IPermissionProvider
{
    public static readonly Permission ManageGoogleTagManager = Permissions.ManageGoogleTagManager;

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageGoogleTagManager,
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
