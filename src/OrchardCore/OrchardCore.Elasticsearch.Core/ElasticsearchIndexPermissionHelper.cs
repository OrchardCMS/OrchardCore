using System.Collections.Concurrent;
using OrchardCore.Indexing.Core;
using OrchardCore.Modules;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Elasticsearch;

public static class ElasticsearchIndexPermissionHelper
{
    private const string PermissionNamePrefix = "QueryElasticsearch";
    private const string PermissionNameSuffix = "Index";

    private static readonly Permission s_indexPermissionTemplate =
        new(PermissionNamePrefix + "{0}" + PermissionNameSuffix, "Query Elasticsearch {0} Index", [Permissions.ManageElasticIndexes]);

    private static readonly ConcurrentDictionary<string, Permission> s_permissions = [];

    [Obsolete($"Use {nameof(IndexingPermissions)}.{nameof(IndexingPermissions.CreateDynamicPermission)} instead.")]
    public static Permission GetElasticIndexPermission(string indexName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);

        return s_permissions.GetOrAdd(indexName, indexName => new Permission(
                string.Format(s_indexPermissionTemplate.Name, indexName),
                string.Format(s_indexPermissionTemplate.Description, indexName),
                s_indexPermissionTemplate.ImpliedBy));
    }

    internal static bool IsElasticsearchIndexPermissionClaim(RoleClaim claim) =>
        claim.ClaimType == nameof(Permission) &&
        claim.ClaimValue.StartsWithOrdinalIgnoreCase(PermissionNamePrefix) &&
        claim.ClaimValue.EndsWithOrdinalIgnoreCase(PermissionNameSuffix);

    internal static string GetIndexNameFromPermissionName(string permissionName) =>
        permissionName[PermissionNamePrefix.Length..^PermissionNameSuffix.Length];
}
