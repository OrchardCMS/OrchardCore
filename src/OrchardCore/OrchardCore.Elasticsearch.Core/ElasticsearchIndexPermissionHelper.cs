using OrchardCore.Modules;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Elasticsearch;

public static class ElasticsearchIndexPermissionHelper
{
    private const string PermissionNamePrefix = "QueryElasticsearch";
    private const string PermissionNameSuffix = "Index";

    internal static bool IsElasticsearchIndexPermissionClaim(RoleClaim claim) =>
        claim.ClaimType == nameof(Permission) &&
        claim.ClaimValue.StartsWithOrdinalIgnoreCase(PermissionNamePrefix) &&
        claim.ClaimValue.EndsWithOrdinalIgnoreCase(PermissionNameSuffix);

    internal static string GetIndexNameFromPermissionName(string permissionName) =>
        permissionName[PermissionNamePrefix.Length..^PermissionNameSuffix.Length];
}
