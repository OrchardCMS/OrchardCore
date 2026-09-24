using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.AzureAI.Deployment;

public class AzureAISearchIndexRebuildDeploymentStep : DeploymentStep
{
    public AzureAISearchIndexRebuildDeploymentStep()
    {
        Name = "AzureAISearchIndexRebuild";
        Category = LocalizedString.Create("Search");
        Title = LocalizedString.Create("Rebuild Azure AI Search Indices");
    }

    public bool IncludeAll { get; set; }

    public string[] Indices { get; set; }
}
