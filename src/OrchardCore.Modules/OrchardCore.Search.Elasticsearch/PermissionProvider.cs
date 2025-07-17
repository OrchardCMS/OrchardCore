using OrchardCore.Indexing;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Elasticsearch;

public sealed class PermissionProvider : IPermissionProvider
{
    private readonly IIndexProfileStore _indexStore;

    public PermissionProvider(IIndexProfileStore indexStore)
    {
        _indexStore = indexStore;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var permissions = new List<Permission>()
        {
            ElasticsearchPermissions.ManageElasticIndexes,
            ElasticsearchPermissions.QueryElasticApi,
        };

        var elasticIndexSettings = await _indexStore.GetByProviderAsync(ElasticsearchConstants.ProviderName);

        foreach (var index in elasticIndexSettings)
        {
            permissions.Add(ElasticsearchIndexPermissionHelper.GetElasticIndexPermission(index.IndexName));
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
                ElasticsearchPermissions.ManageElasticIndexes,
            ],
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Editor,
            Permissions =
            [
                ElasticsearchPermissions.QueryElasticApi,
            ],
        },
    ];
}
