using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Elasticsearch;

public class Permissions : IPermissionProvider
{
    private readonly ElasticIndexSettingsService _elasticIndexSettingsService;

    public static readonly Permission ManageElasticIndexes = ElasticsearchIndexPermissionHelper.ManageElasticIndexes;
    public static readonly Permission QueryElasticApi = new("QueryElasticsearchApi", "Query Elasticsearch Api", new[] { ManageElasticIndexes });

    public Permissions(ElasticIndexSettingsService elasticIndexSettingsService)
    {
        _elasticIndexSettingsService = elasticIndexSettingsService;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var permissions = new List<Permission>
        {
            ManageElasticIndexes,
            QueryElasticApi,
        };

        var elasticIndexSettings = await _elasticIndexSettingsService.GetSettingsAsync();

        foreach (var index in elasticIndexSettings)
        {
            permissions.Add(ElasticsearchIndexPermissionHelper.GetElasticIndexPermission(index.IndexName));
        }

        return permissions;
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _allStereotypes;

    private readonly static IEnumerable<PermissionStereotype> _allStereotypes =
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions =
            [
                ManageElasticIndexes
            ],
        },
        new PermissionStereotype
        {
            Name = "Editor",
            Permissions =
            [
                QueryElasticApi,
            ],
        },
    ];
}
