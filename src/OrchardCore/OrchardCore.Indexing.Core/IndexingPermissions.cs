using OrchardCore.Localization;
using System.Collections.Concurrent;
using OrchardCore.Indexing.Models;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Indexing.Core;

public static class IndexingPermissions
{
    public static readonly Permission QuerySearchIndex = new("QuerySearchIndex", new LocalizationSource("Query any index", typeof(IndexingPermissions)));

    public static readonly Permission ManageIndexes = new("ManageIndexes", new LocalizationSource("Manage Indexes", typeof(IndexingPermissions)));

    private static readonly Permission s_indexPermissionTemplate =
        new("QueryIndex_{0}", "Query '{0}' Index", [ManageIndexes, QuerySearchIndex]);

    private static readonly ConcurrentDictionary<string, Permission> s_permissions = [];

    public static Permission CreateDynamicPermission(IndexProfile indexProfile)
    {
        ArgumentNullException.ThrowIfNull(indexProfile);

        return s_permissions.GetOrAdd(indexProfile.Id, indexId => new Permission(
            string.Format(s_indexPermissionTemplate.Name, indexProfile.Name),
            string.Format(s_indexPermissionTemplate.Description.Value, indexProfile.Name),
            s_indexPermissionTemplate.ImpliedBy));
    }
}
