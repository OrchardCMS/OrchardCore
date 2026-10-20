using OrchardCore.Indexing;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Lucene;

public sealed class Permissions : IPermissionProvider
{
    private readonly IIndexProfileStore _indexStore;

    public Permissions(IIndexProfileStore indexStore)
    {
        _indexStore = indexStore;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var permissions = new List<Permission>()
        {
            LuceneSearchPermissions.ManageLuceneIndexes,
            LuceneSearchPermissions.QueryLuceneApi,
        };

        var indexes = await _indexStore.GetByProviderAsync(LuceneConstants.ProviderName);

        foreach (var index in indexes)
        {
            permissions.Add(LuceneIndexPermissionHelper.GetLuceneIndexPermission(index.IndexName));
        }

        return permissions;
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions =
            [
                LuceneSearchPermissions.ManageLuceneIndexes,
            ],
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Editor,
            Permissions =
            [
                LuceneSearchPermissions.QueryLuceneApi,
            ],
        },
    ];
}
