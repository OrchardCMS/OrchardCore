using System.Collections.Concurrent;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Elasticsearch;

public static class ElasticsearchIndexPermissionHelper
{
    private static readonly Permission _indexPermissionTemplate = new("QueryElasticsearch{0}Index", "Query Elasticsearch {0} Index", [Permissions.ManageElasticIndexes]);

    private static readonly ConcurrentDictionary<string, Permission> _permissions = [];

    public static Permission GetElasticIndexPermission(string indexName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);

        return _permissions.GetOrAdd(indexName, indexName => new Permission(
                string.Format(_indexPermissionTemplate.Name, indexName),
                string.Format(_indexPermissionTemplate.Description, indexName),
                _indexPermissionTemplate.ImpliedBy));
    }
}
