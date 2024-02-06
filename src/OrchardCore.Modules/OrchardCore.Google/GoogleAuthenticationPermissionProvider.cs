using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Google;

public class GoogleAuthenticationPermissionProvider : IPermissionProvider
{
    public static readonly Permission ManageGoogleAuthentication = Permissions.ManageGoogleAuthentication;

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageGoogleAuthentication,
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
