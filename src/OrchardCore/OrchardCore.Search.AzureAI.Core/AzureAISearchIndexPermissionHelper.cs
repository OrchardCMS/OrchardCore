using System.Collections.Concurrent;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.AzureAI;

public static class AzureAISearchIndexPermissionHelper
{
    public static readonly Permission ManageAzureAISearchIndexes =
        new("ManageAzureAISearchIndexes", "Manage Azure AI Search Indexes");

    private static readonly Permission _indexPermissionTemplate =
        new("QueryAzureAISearchIndex_{0}", "Query Azure AI Search '{0}' Index", [ManageAzureAISearchIndexes]);

    private static readonly ConcurrentDictionary<string, Permission> _permissions = [];

    public static Permission GetPermission(string indexName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);

        return _permissions.GetOrAdd(indexName, indexName => new Permission(
                string.Format(_indexPermissionTemplate.Name, indexName),
                string.Format(_indexPermissionTemplate.Description, indexName),
                _indexPermissionTemplate.ImpliedBy));
    }
}
