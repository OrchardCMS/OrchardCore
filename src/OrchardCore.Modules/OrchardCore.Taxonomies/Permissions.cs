using OrchardCore.Security.Permissions;

namespace OrchardCore.Taxonomies;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageTaxonomies = new("ManageTaxonomy", "Manage taxonomies");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageTaxonomies,
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
