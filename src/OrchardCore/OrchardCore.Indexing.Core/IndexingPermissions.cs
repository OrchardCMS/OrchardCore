using OrchardCore.Localization;
using System.Collections.Concurrent;
using OrchardCore.Indexing.Models;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Indexing.Core;

public static class IndexingPermissions
{
    public static readonly Permission QuerySearchIndex = new("QuerySearchIndex", LocalizationSource.Create("Query any index", typeof(IndexingPermissions)));

    public static readonly Permission ManageIndexes = new("ManageIndexes", LocalizationSource.Create("Manage Indexes", typeof(IndexingPermissions)));

    private static readonly PermissionTemplate s_indexPermissionTemplate =
        new("QueryIndex_{0}", "Query '{0}' Index", ManageIndexes, QuerySearchIndex);

    private static readonly ConcurrentDictionary<string, Permission> s_permissions = [];

    public static Permission CreateDynamicPermission(IndexProfile indexProfile)
    {
        ArgumentNullException.ThrowIfNull(indexProfile);

        return s_permissions.GetOrAdd(
            indexProfile.Id,
            _ => s_indexPermissionTemplate.CreateDynamicPermission(indexProfile.Name));
    }
}
