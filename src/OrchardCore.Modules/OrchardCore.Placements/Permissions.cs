using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Placements;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManagePlacements = new("ManagePlacements", "Manage placements");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManagePlacements,
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
        new PermissionStereotype
        {
            Name = "Editor",
            Permissions = _allPermissions,
        },
    ];
}
