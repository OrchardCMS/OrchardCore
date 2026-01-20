using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Sitemaps;

public class Permissions : IPermissionProvider
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
            Name = "Administrator",
            Permissions = _allPermissions,
        },
    ];
}
