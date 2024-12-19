using OrchardCore.Security.Permissions;

namespace OrchardCore.Indexing;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageIndexes = new("ManageIndexes", "Manage Indexes");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageIndexes,
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
