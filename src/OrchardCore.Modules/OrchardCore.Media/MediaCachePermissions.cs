using OrchardCore.Security.Permissions;

namespace OrchardCore.Media;

public sealed class MediaCachePermissions : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        MediaPermissions.ManageAssetCache,
    ];

    [Obsolete("This will be removed in a future release. Instead use 'MediaPermissions.ManageAssetCache'.")]
    public static readonly Permission ManageAssetCache = new("ManageAssetCache", "Manage Asset Cache Folder");

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
