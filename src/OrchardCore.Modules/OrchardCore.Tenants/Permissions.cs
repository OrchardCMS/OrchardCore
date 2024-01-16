using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Tenants;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageTenants = new("ManageTenants", "Manage tenants");
    public static readonly Permission ManageTenantFeatureProfiles = new("ManageTenantFeatureProfiles", "Manage tenant feature profiles");

    private static readonly IEnumerable<Permission> _allPermissions =
    [
        ManageTenants,
        ManageTenantFeatureProfiles,
    ];

    private static readonly IEnumerable<PermissionStereotype> _stereotypes =
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _allPermissions,
        },
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _stereotypes;
}
