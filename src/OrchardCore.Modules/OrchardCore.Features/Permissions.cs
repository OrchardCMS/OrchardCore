using OrchardCore.Security.Permissions;

namespace OrchardCore.Features;

public sealed class Permissions : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        FeaturesPermissions.ManageFeatures,
    ];

    [Obsolete("This will be removed in a future release. Instead use 'FeaturesPermissions.ManageFeatures'.")]
    public static readonly Permission ManageFeatures = new("ManageFeatures", "Manage Features");

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
