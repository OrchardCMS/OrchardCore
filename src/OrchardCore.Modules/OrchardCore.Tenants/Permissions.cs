using OrchardCore.Security.Permissions;

namespace OrchardCore.Tenants;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageTenants = new("ManageTenants", "Manage tenants");
    public static readonly Permission ManageTenantFeatureProfiles = new("ManageTenantFeatureProfiles", "Manage tenant feature profiles");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageTenants,
        ManageTenantFeatureProfiles,
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
