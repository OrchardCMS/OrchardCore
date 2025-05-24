using OrchardCore.Indexing.Core;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Indexing;

public sealed class Permissions : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        IndexingPermissions.ManageIndexes,
    ];

    private readonly IIndexEntityStore _indexEntityStore;

    public Permissions(IIndexEntityStore indexEntityStore)
    {
        _indexEntityStore = indexEntityStore;
    }


    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var result = new List<Permission>();

        foreach (var index in await _indexEntityStore.GetAllAsync())
        {
            result.Add(IndexingPermissions.CreateDynamicPermission(index.IndexName));
        }

        return result;
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
