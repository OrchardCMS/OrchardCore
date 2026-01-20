using OrchardCore.Deployment;
using OrchardCore.Search.AzureAI.Recipes;

namespace OrchardCore.Search.AzureAI.Deployment;

public class AzureAISearchIndexDeploymentStep : DeploymentStep
{
    public AzureAISearchIndexDeploymentStep()
    {
        Name = AzureAISearchIndexSettingsStep.Name;
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
