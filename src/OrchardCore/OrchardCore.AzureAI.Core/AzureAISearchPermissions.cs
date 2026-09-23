using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AzureAI;

public static class AzureAISearchPermissions
{
    public static readonly Permission ManageAzureAISearchISettings = new("ManageAzureAISearchISettings", LocalizedString.Create("Manage Azure AI Search Settings", typeof(AzureAISearchPermissions)));
}
