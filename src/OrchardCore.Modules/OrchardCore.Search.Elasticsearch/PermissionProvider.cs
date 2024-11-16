using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Elasticsearch;

public sealed class PermissionProvider : IPermissionProvider
{
    private readonly ElasticsearchIndexSettingsService _elasticIndexSettingsService;

    public PermissionProvider(ElasticsearchIndexSettingsService elasticIndexSettingsService)
    {
        _elasticIndexSettingsService = elasticIndexSettingsService;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var permissions = new List<Permission>()
        {
            Permissions.ManageElasticIndexes,
            Permissions.QueryElasticApi,
        };

        var elasticIndexSettings = await _elasticIndexSettingsService.GetSettingsAsync();

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
                Permissions.ManageElasticIndexes,
            ],
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Editor,
            Permissions =
            [
                Permissions.QueryElasticApi,
            ],
        },
    ];
}
