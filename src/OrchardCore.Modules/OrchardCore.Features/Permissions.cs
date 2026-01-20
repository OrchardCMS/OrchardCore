using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Features;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageFeatures = new("ManageFeatures", "Manage Features");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageFeatures,
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
