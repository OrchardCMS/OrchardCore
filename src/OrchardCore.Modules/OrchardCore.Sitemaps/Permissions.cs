using OrchardCore.Security.Permissions;

namespace OrchardCore.Sitemaps;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageSitemaps = new("ManageSitemaps", "Manage sitemaps");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageSitemaps,
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
