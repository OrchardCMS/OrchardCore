using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.AzureAI;

public sealed class AzureAISearchPermissions
{
    public static readonly Permission ManageAzureAISearchIndexes = new("ManageAzureAISearchIndexes", "Manage Azure AI Search Indexes");

}
