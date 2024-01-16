using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Taxonomies;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageTaxonomies = new("ManageTaxonomy", "Manage taxonomies");

    private static readonly IEnumerable<Permission> _allPermissions =
    [
        ManageTaxonomies,
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
