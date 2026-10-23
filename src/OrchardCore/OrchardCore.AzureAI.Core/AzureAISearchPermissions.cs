using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AzureAI;

public static class AzureAISearchPermissions
{
    public static readonly Permission ManageAzureAISearchSettings = new("ManageAzureAISearchSettings", LocalizationSource.Create("Manage Azure AI Search Settings", typeof(AzureAISearchPermissions)));
}
