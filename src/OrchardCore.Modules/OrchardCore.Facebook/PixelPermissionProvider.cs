using OrchardCore.Security.Permissions;

namespace OrchardCore.Facebook;

public sealed class PixelPermissionProvider : IPermissionProvider
{
    public static readonly Permission ManageFacebookPixelPermission = FacebookConstants.ManageFacebookPixelPermission;

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageFacebookPixelPermission,
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
