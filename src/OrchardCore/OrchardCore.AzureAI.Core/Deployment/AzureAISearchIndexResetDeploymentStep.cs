using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.AzureAI.Deployment;

public class AzureAISearchIndexResetDeploymentStep : DeploymentStep
{
    public AzureAISearchIndexResetDeploymentStep()
    {
        Name = "AzureAISearchIndexReset";
    }

    public AzureAISearchIndexResetDeploymentStep(IStringLocalizer<AzureAISearchIndexResetDeploymentStep> S)
        : this()
    {
        Category = S["Search"];
        DisplayName = S["Reset Azure AI Search Indices"];
    }

    public bool IncludeAll { get; set; }

    public string[] Indices { get; set; }
}
