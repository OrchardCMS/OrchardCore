using OrchardCore.Security.Core;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Security;

public sealed class SecurityPermissions : IPermissionProvider
{
    [Obsolete("This will be removed in a future release. Instead use 'SecurityConstants.Permissions.ManageSecurityHeadersSettings'.")]
    public static readonly Permission ManageSecurityHeadersSettings = SecurityConstants.Permissions.ManageSecurityHeadersSettings;

    private readonly IEnumerable<Permission> _allPermissions =
    [
        SecurityConstants.Permissions.ManageSecurityHeadersSettings,
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
