using OrchardCore.Deployment;
using OrchardCore.Search.AzureAI.Recipes;

namespace OrchardCore.Search.AzureAI.Deployment;

public class AzureAISearchSettingsDeploymentStep : DeploymentStep
{
    public AzureAISearchSettingsDeploymentStep()
    {
        Name = AzureAISearchIndexSettingsStep.Name;
    }
}
