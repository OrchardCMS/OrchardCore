using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.AzureAI;

public static class AzureAISearchPermissions
{
    public static readonly Permission ManageAzureAISearchISettings = new("ManageAzureAISearchISettings", "Manage Azure AI Search Settings");
}
