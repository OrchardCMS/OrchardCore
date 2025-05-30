using System.Collections.Concurrent;
using OrchardCore.Indexing.Models;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Indexing.Core;

public static class IndexingPermissions
{
    public static readonly Permission ManageIndexes = new("ManageIndexes", "Manage Indexes");

    private static readonly Permission _indexPermissionTemplate =
        new("QueryIndex_{0}", "Query '{0}' Index using '{1}' provider", [ManageIndexes]);

    private static readonly ConcurrentDictionary<string, Permission> _permissions = [];

    public static Permission CreateDynamicPermission(IndexEntity index)
    {
        ArgumentNullException.ThrowIfNull(index);

        return _permissions.GetOrAdd(index.Id, indexId => new Permission(
                string.Format(_indexPermissionTemplate.Name, index.IndexFullName),
                string.Format(_indexPermissionTemplate.Description, index.IndexName, index.ProviderName),
                _indexPermissionTemplate.ImpliedBy));
    }
}
