using System.Collections.Concurrent;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Indexing.Core;

public static class IndexingPermissions
{
    public static readonly Permission ManageIndexes = new("ManageIndexes", "Manage Indexes");

    private static readonly Permission _indexPermissionTemplate =
        new("QueryIndex_{0}", "Query '{0}' Index", [ManageIndexes]);

    private static readonly ConcurrentDictionary<string, Permission> _permissions = [];

    public static Permission CreateDynamicPermission(string indexName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);

        return _permissions.GetOrAdd(indexName, indexName => new Permission(
                string.Format(_indexPermissionTemplate.Name, indexName),
                string.Format(_indexPermissionTemplate.Description, indexName),
                _indexPermissionTemplate.ImpliedBy));
    }
}
