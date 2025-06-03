using OrchardCore.Indexing.Core;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Indexing;

public sealed class IndexingPermissionsProvider : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        IndexingPermissions.ManageIndexes,
    ];

    private readonly IIndexEntityStore _store;

    public IndexingPermissionsProvider(IIndexEntityStore store)
    {
        _store = store;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var indexes = await _store.GetAllAsync();

        var permissions = new List<Permission>(indexes.Count() + 1)
        {
            IndexingPermissions.ManageIndexes,
        };

        foreach (var index in await _store.GetAllAsync())
        {
            permissions.Add(IndexingPermissions.CreateDynamicPermission(index));
        }

        return permissions;
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = _allPermissions,
        },
    ];
}
