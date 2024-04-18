using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Google;

[Feature(GoogleConstants.Features.GoogleAuthentication)]
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
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = _allPermissions,
        },
    ];
}
