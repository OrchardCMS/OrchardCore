using System.Collections.Concurrent;
using OrchardCore.Indexing.Core;
using OrchardCore.Modules;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.OpenSearch;

public static class OpenSearchIndexPermissionHelper
{
    private const string PermissionNamePrefix = "QueryOpenSearch";
    private const string PermissionNameSuffix = "Index";

    private static readonly Permission _indexPermissionTemplate =
        new(PermissionNamePrefix + "{0}" + PermissionNameSuffix, "Query OpenSearch {0} Index", [Permissions.ManageOpenSearchIndexes]);

    private static readonly ConcurrentDictionary<string, Permission> _permissions = [];

    [Obsolete($"Use {nameof(IndexingPermissions)}.{nameof(IndexingPermissions.CreateDynamicPermission)} instead.")]
    public static Permission GetOpenSearchIndexPermission(string indexName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);

        return _permissions.GetOrAdd(indexName, indexName => new Permission(
                string.Format(_indexPermissionTemplate.Name, indexName),
                string.Format(_indexPermissionTemplate.Description, indexName),
                _indexPermissionTemplate.ImpliedBy));
    }

    internal static bool IsOpenSearchIndexPermissionClaim(RoleClaim claim) =>
        claim.ClaimType == nameof(Permission) &&
        claim.ClaimValue.StartsWithOrdinalIgnoreCase(PermissionNamePrefix) &&
        claim.ClaimValue.EndsWithOrdinalIgnoreCase(PermissionNameSuffix);

    internal static string GetIndexNameFromPermissionName(string permissionName) =>
        permissionName[PermissionNamePrefix.Length..^PermissionNameSuffix.Length];
}
