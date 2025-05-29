using OrchardCore.Indexing;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Lucene;

public sealed class Permissions : IPermissionProvider
{
    private readonly IIndexEntityStore _indexStore;

    [Obsolete("This will be removed in a future release. Instead use 'LuceneSearchPermissions.ManageLuceneIndexes'.")]
    public static readonly Permission ManageLuceneIndexes = LuceneSearchPermissions.ManageLuceneIndexes;

    [Obsolete("This will be removed in a future release. Instead use 'LuceneSearchPermissions.QueryLuceneApi'.")]
    public static readonly Permission QueryLuceneApi = LuceneSearchPermissions.QueryLuceneApi;

    public Permissions(IIndexEntityStore indexStore)
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

        var indexes = await _indexStore.GetAsync(LuceneConstants.ProviderName);

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
