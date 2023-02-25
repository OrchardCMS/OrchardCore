using System;
using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Lucene;

public class LuceneIndexPermissionHelper
{
    public static readonly Permission ManageLuceneIndexes = new("ManageLuceneIndexes", "Manage Lucene Indexes");

    private static readonly Permission _indexPermissionTemplate = new("QueryLucene{0}Index", "Query Lucene {0} Index", new[] { ManageLuceneIndexes });

    private static readonly Dictionary<string, Permission> _permissions = new();

    public static Permission GetLuceneIndexPermission(string indexName)
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
