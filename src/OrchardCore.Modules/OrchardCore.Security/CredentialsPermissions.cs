using OrchardCore.Security.Core;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Security;

public sealed class CredentialsPermissions : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        SecurityConstants.Permissions.ManageCredentials,
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
