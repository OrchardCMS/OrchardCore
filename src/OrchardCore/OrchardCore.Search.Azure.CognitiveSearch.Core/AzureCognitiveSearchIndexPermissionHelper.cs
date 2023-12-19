using System;
using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Elasticsearch;

public class AzureCognitiveSearchIndexPermissionHelper
{
    public static readonly Permission ManageAzureCognitiveSearchIndexes = new("ManageAzureCognitiveSearchIndexes", "Manage Azure Cognitive Search Indexes");

    private static readonly Permission _indexPermissionTemplate = new("QueryAzureCognitiveSearchIndex_{0}", "Query Azure Cognitive Search '{0}' Index", new[] { ManageAzureCognitiveSearchIndexes });

    private static readonly Dictionary<string, Permission> _permissions = new();

    public static Permission GetElasticIndexPermission(string indexName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName, nameof(indexName));

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
