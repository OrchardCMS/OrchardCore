using System.Collections.Concurrent;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Lucene;

public static class LuceneIndexPermissionHelper
{
    public static readonly Permission ManageLuceneIndexes = new("ManageLuceneIndexes", "Manage Lucene Indexes");

    private static readonly Permission _indexPermissionTemplate = new("QueryLucene{0}Index", "Query Lucene {0} Index", new[] { ManageLuceneIndexes });

    private static readonly ConcurrentDictionary<string, Permission> _permissions = [];

    public static Permission GetLuceneIndexPermission(string indexName)
    {
        if (string.IsNullOrEmpty(indexName))
        {
            throw new ArgumentException($"{nameof(indexName)} cannot be null or empty");
        }

        return _permissions.GetOrAdd(indexName, indexName => new Permission(
                string.Format(_indexPermissionTemplate.Name, indexName),
                string.Format(_indexPermissionTemplate.Description, indexName),
                _indexPermissionTemplate.ImpliedBy));
    }
}
