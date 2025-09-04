using OrchardCore.Security.Permissions;

namespace OrchardCore.ReverseProxy;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageReverseProxySettings = new("ManageReverseProxySettings", "Manage Reverse Proxy Settings");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageReverseProxySettings,
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
