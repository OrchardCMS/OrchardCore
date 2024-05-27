using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Elasticsearch;

public class Permissions : IPermissionProvider
{
    [Obsolete("This property will be removed in future release. Instead use 'ElasticsearchIndexPermissionHelper.ManageElasticIndexes'.")]
    public static readonly Permission ManageElasticIndexes = ElasticsearchIndexPermissionHelper.ManageElasticIndexes;

    public static readonly Permission QueryElasticApi = new("QueryElasticsearchApi", "Query Elasticsearch Api", new[] { ElasticsearchIndexPermissionHelper.ManageElasticIndexes });

    private readonly ElasticIndexSettingsService _elasticIndexSettingsService;

    public Permissions(ElasticIndexSettingsService elasticIndexSettingsService)
    {
        _elasticIndexSettingsService = elasticIndexSettingsService;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var permissions = new List<Permission>()
        {
            ElasticsearchIndexPermissionHelper.ManageElasticIndexes,
            QueryElasticApi,
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
                ElasticsearchIndexPermissionHelper.ManageElasticIndexes,
            ],
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Editor,
            Permissions =
            [
                QueryElasticApi,
            ],
        },
    ];
}
