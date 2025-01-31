using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.AzureAI;

public static class AzureAISearchPermissions
{
    public static readonly Permission ManageAzureAISearchIndexes = new("ManageAzureAISearchIndexes", "Manage Azure AI Search Indexes");
}
