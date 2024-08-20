using OrchardCore.Security.Permissions;

namespace OrchardCore.Features;

public sealed class Permissions : IPermissionProvider
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
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = _allPermissions,
        },
    ];
}
