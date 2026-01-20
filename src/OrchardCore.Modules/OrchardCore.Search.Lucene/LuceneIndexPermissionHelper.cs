using System.Collections.Concurrent;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Lucene;

public static class LuceneIndexPermissionHelper
{
    private static readonly Permission _indexPermissionTemplate = new("QueryLucene{0}Index", "Query Lucene {0} Index", new[] { LuceneSearchPermissions.ManageLuceneIndexes });

    private static readonly ConcurrentDictionary<string, Permission> _permissions = [];

    [Obsolete("This will be removed in a future release. Instead use 'LuceneSearchPermissions.ManageLuceneIndexes'.")]
    public static readonly Permission ManageLuceneIndexes = LuceneSearchPermissions.ManageLuceneIndexes;

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
