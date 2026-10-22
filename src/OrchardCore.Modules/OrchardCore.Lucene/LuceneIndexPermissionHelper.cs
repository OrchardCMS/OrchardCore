using OrchardCore.Localization;
using System.Collections.Concurrent;
using OrchardCore.Modules;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Lucene;

public static class LuceneIndexPermissionHelper
{
    private const string PermissionNamePrefix = "QueryLucene";
    private const string PermissionNameSuffix = "Index";

    private static readonly PermissionTemplate s_indexPermissionTemplate = new(
        "QueryLucene{0}Index",
        LocalizationSource.Create("Query Lucene {0} Index", typeof(LuceneIndexPermissionHelper)), 
        LuceneSearchPermissions.ManageLuceneIndexes);

    private static readonly ConcurrentDictionary<string, Permission> s_permissions = [];

    public static Permission GetLuceneIndexPermission(string indexName)
    {
        if (string.IsNullOrEmpty(indexName))
        {
            throw new ArgumentException($"{nameof(indexName)} cannot be null or empty");
        }

        return s_permissions.GetOrAdd(indexName, s_indexPermissionTemplate.CreateDynamicPermission);
    }

    internal static bool IsLuceneIndexPermissionClaim(RoleClaim claim) =>
        claim.ClaimType == nameof(Permission) &&
        claim.ClaimValue.StartsWithOrdinalIgnoreCase(PermissionNamePrefix) &&
        claim.ClaimValue.EndsWithOrdinalIgnoreCase(PermissionNameSuffix);

    internal static string GetIndexNameFromPermissionName(string permissionName) =>
        permissionName[PermissionNamePrefix.Length..^PermissionNameSuffix.Length];
}
