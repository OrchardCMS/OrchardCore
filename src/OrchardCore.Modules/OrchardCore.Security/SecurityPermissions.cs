using OrchardCore.Security.Permissions;

namespace OrchardCore.Security;

public sealed class SecurityPermissions : IPermissionProvider
{
    public static readonly Permission ManageSecurityHeadersSettings = new("ManageSecurityHeadersSettings", "Manage Security Headers Settings");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageSecurityHeadersSettings,
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
