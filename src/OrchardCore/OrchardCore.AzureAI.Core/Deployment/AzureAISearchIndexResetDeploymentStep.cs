using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.AzureAI.Deployment;

public class AzureAISearchIndexResetDeploymentStep : DeploymentStep
{
    public AzureAISearchIndexResetDeploymentStep()
    {
        Name = "AzureAISearchIndexReset";
        Category = LocalizedString.Create("Search");
    }

    public bool IncludeAll { get; set; }

    public string[] Indices { get; set; }
}
