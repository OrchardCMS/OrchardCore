using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Lucene;

public static class LuceneIndexPermissionHelper
{
    public static readonly Permission ManageLuceneIndexes = new("ManageLuceneIndexes", "Manage Lucene Indexes");

    private static readonly Permission _indexPermissionTemplate = new("QueryLucene{0}Index", "Query Lucene {0} Index", new[] { ManageLuceneIndexes });

    private static readonly Dictionary<string, Permission> _permissions = [];

    public static Permission GetLuceneIndexPermission(string indexName)
    {
        if (string.IsNullOrEmpty(indexName))
        {
            throw new ArgumentException($"{nameof(indexName)} cannot be null or empty");
        }

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
