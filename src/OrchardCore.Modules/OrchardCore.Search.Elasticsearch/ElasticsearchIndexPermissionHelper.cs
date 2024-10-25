using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Elasticsearch;

public static class ElasticsearchIndexPermissionHelper
{
    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Search.Elasticsearch.Permissions.ManageElasticIndexes'.")]
    public static readonly Permission ManageElasticIndexes = Permissions.ManageElasticIndexes;

    private static readonly Permission _indexPermissionTemplate = new("QueryElasticsearch{0}Index", "Query Elasticsearch {0} Index", [Permissions.ManageElasticIndexes]);

    private static readonly Dictionary<string, Permission> _permissions = [];

    public static Permission GetElasticIndexPermission(string indexName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);

        if (!_permissions.TryGetValue(indexName, out var permission))
        {
            permission = new Permission(
                string.Format(_indexPermissionTemplate.Name, indexName),
                string.Format(_indexPermissionTemplate.Description, indexName),
                _indexPermissionTemplate.ImpliedBy
            );

            _permissions.Add(indexName, permission);
        }

        return permission;
    }
}
