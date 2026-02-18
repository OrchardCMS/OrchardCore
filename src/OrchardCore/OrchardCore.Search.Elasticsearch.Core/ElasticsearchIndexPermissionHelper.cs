using OrchardCore.Indexing.Core;
using OrchardCore.Modules;
using OrchardCore.Security;
using System.Collections.Concurrent;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Elasticsearch;

public static class ElasticsearchIndexPermissionHelper
{
    private const string PermissionNamePrefix = "QueryElasticsearch";
    private const string PermissionNameSuffix = "Index";
    
    private static readonly Permission _indexPermissionTemplate = 
        new(PermissionNamePrefix + "{0}" + PermissionNameSuffix, "Query Elasticsearch {0} Index", [Permissions.ManageElasticIndexes]);

    private static readonly ConcurrentDictionary<string, Permission> _permissions = [];

    [Obsolete($"Use {nameof(IndexingPermissions)}.{nameof(IndexingPermissions.CreateDynamicPermission)} instead.")]
    public static Permission GetElasticIndexPermission(string indexName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);

        return _permissions.GetOrAdd(indexName, indexName => new Permission(
                string.Format(_indexPermissionTemplate.Name, indexName),
                string.Format(_indexPermissionTemplate.Description, indexName),
                _indexPermissionTemplate.ImpliedBy));
    }
    
    internal static bool IsElasticsearchIndexPermissionClaim(RoleClaim claim) =>
        claim.ClaimValue.StartsWithOrdinalIgnoreCase(PermissionNamePrefix) &&
        claim.ClaimValue.EndsWithOrdinalIgnoreCase(PermissionNameSuffix);

    internal static string GetIndexNameFromPermissionName(string permissionName) =>
        permissionName[PermissionNamePrefix.Length .. ^PermissionNameSuffix.Length];
}
