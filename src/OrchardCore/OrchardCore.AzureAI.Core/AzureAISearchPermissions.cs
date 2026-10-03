using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AzureAI;

public static class AzureAISearchPermissions
{
    public static readonly Permission ManageAzureAISearchISettings = new("ManageAzureAISearchISettings", new LocalizationSource("Manage Azure AI Search Settings", typeof(AzureAISearchPermissions)));
}
