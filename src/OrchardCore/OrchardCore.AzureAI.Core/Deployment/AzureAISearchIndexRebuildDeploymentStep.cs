using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.AzureAI.Deployment;

public class AzureAISearchIndexRebuildDeploymentStep : DeploymentStep
{
    public AzureAISearchIndexRebuildDeploymentStep()
    {
        Name = "AzureAISearchIndexRebuild";
    }

    public AzureAISearchIndexRebuildDeploymentStep(IStringLocalizer<AzureAISearchIndexRebuildDeploymentStep> S)
        : this()
    {
        Category = S["Search"];
        DisplayName = S["Rebuild Azure AI Search Indices"];
    }

    public bool IncludeAll { get; set; }

    public string[] Indices { get; set; }
}
