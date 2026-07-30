using System.Collections.Concurrent;
using OrchardCore.Modules;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Lucene;

public static class LuceneIndexPermissionHelper
{
    private const string PermissionNamePrefix = "QueryLucene";
    private const string PermissionNameSuffix = "Index";

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

    internal static bool IsLuceneIndexPermissionClaim(RoleClaim claim) =>
        claim.ClaimType == nameof(Permission) &&
        claim.ClaimValue.StartsWithOrdinalIgnoreCase(PermissionNamePrefix) &&
        claim.ClaimValue.EndsWithOrdinalIgnoreCase(PermissionNameSuffix);

    internal static string GetIndexNameFromPermissionName(string permissionName) =>
        permissionName[PermissionNamePrefix.Length..^PermissionNameSuffix.Length];
}
