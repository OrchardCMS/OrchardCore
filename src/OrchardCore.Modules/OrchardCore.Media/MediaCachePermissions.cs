using OrchardCore.Security.Permissions;

namespace OrchardCore.Media;

public sealed class MediaCachePermissions : IPermissionProvider
{
    public static readonly Permission ManageAssetCache = new("ManageAssetCache", "Manage Asset Cache Folder");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageAssetCache,
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
