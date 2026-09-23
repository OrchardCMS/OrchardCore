using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Tenants;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageTenants = new("ManageTenants", LocalizedString.Create("Manage tenants", typeof(Permissions)));
    public static readonly Permission ManageTenantFeatureProfiles = new("ManageTenantFeatureProfiles", LocalizedString.Create("Manage tenant feature profiles", typeof(Permissions)));

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
