using OrchardCore.Security.Permissions;

namespace OrchardCore.Sitemaps;

public sealed class Permissions : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        SitemapsPermissions.ManageSitemaps,
    ];

    [Obsolete("This will be removed in a future release. Instead use 'SitemapsPermissions.ManageSitemaps'.")]
    public static readonly Permission ManageSitemaps = SitemapsPermissions.ManageSitemaps;

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
