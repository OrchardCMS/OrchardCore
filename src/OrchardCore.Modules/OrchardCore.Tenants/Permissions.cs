using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Tenants;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageTenants = new("ManageTenants", "Manage tenants");
    public static readonly Permission ManageTenantFeatureProfiles = new("ManageTenantFeatureProfiles", "Manage tenant feature profiles");

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _allStereotypes;

    private readonly static IEnumerable<PermissionStereotype> _allStereotypes =
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _allPermissions,
        },
    ];

    private readonly static IEnumerable<Permission> _allPermissions =
    [
        ManageTenants,
        ManageTenantFeatureProfiles,
    ];
}
