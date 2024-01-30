using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Google;

public class GoogleTagManagerPermissionProvider : IPermissionProvider
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
            Name = "Administrator",
            Permissions = _allPermissions,
        },
    ];
}
