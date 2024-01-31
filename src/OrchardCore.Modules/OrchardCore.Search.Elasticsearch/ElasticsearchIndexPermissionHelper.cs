using System;
using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Elasticsearch;

public class ElasticsearchIndexPermissionHelper
{
    public static readonly Permission ManageElasticIndexes = new("ManageElasticIndexes", "Manage Elasticsearch Indexes");

    private static readonly Permission _indexPermissionTemplate = new("QueryElasticsearch{0}Index", "Query Elasticsearch {0} Index", new[] { ManageElasticIndexes });

    private static readonly Dictionary<string, Permission> _permissions = [];

    public static Permission GetElasticIndexPermission(string indexName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName, nameof(indexName));

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
