using System;
using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Elasticsearch;

public class ElasticsearchIndexPermissionHelper
{
    public static readonly Permission ManageElasticIndexes = new("ManageElasticIndexes", "Manage Elasticsearch Indexes");

    private static readonly Permission _indexPermissionTemplate = new("QueryElasticsearch{0}Index", "Query Elasticsearch {0} Index", new[] { ManageElasticIndexes });

    private static readonly Dictionary<string, Permission> _permissions = new();

    public static Permission GetElasticIndexPermission(string indexName)
    {
        if (String.IsNullOrEmpty(indexName))
        {
            throw new ArgumentException($"{nameof(indexName)} cannot be null or empty");
        }

        if (!_permissions.TryGetValue(indexName, out var permission))
        {
            permission = new Permission(
                String.Format(_indexPermissionTemplate.Name, indexName),
                String.Format(_indexPermissionTemplate.Description, indexName),
                _indexPermissionTemplate.ImpliedBy
            );

            _permissions.Add(indexName, permission);
        }

        return permission;
    }
}
