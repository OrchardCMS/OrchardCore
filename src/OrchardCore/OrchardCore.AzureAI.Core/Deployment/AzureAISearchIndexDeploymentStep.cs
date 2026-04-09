using OrchardCore.Deployment;
using OrchardCore.AzureAI.Recipes;

namespace OrchardCore.AzureAI.Deployment;

public class AzureAISearchIndexDeploymentStep : DeploymentStep
{
    public AzureAISearchIndexDeploymentStep()
    {
        Name = AzureAISearchIndexSettingsStep.Name;
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
