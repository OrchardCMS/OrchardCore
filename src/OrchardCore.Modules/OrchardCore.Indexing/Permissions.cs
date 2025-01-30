using OrchardCore.Security.Permissions;

namespace OrchardCore.Indexing;

public sealed class Permissions : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        IndexingPermissions.ManageIndexes,
    ];

    [Obsolete("This will be removed in a future release. Instead use 'IndexingPermissions.ManageIndexes'.")]
    public static readonly Permission ManageIndexes = new("ManageIndexes", "Manage Indexes");

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
