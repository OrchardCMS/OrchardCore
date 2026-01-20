using OrchardCore.Security.Permissions;

namespace OrchardCore.Https;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageHttps = new("ManageHttps", "Manage HTTPS");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageHttps,
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
