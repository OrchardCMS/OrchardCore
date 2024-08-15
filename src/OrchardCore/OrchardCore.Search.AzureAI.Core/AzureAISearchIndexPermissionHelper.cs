using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.AzureAI;

public static class AzureAISearchIndexPermissionHelper
{
    public static readonly Permission ManageAzureAISearchIndexes =
        new("ManageAzureAISearchIndexes", "Manage Azure AI Search Indexes");

    private static readonly Permission _indexPermissionTemplate =
        new("QueryAzureAISearchIndex_{0}", "Query Azure AI Search '{0}' Index", [ManageAzureAISearchIndexes]);

    private static readonly Dictionary<string, Permission> _permissions = [];

    public static Permission GetPermission(string indexName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);

        if (!_permissions.TryGetValue(indexName, out var permission))
        {
            permission = new Permission(
                string.Format(_indexPermissionTemplate.Name, indexName),
                string.Format(_indexPermissionTemplate.Description, indexName),
                _indexPermissionTemplate.ImpliedBy
            );

            _permissions.Add(indexName, permission);
        }

        return permission;
    }
}
