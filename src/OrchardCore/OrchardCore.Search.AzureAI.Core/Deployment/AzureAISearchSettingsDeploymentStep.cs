using OrchardCore.Deployment;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.Deployment;

public class AzureAISearchSettingsDeploymentStep : DeploymentStep
{
    public AzureAISearchSettingsDeploymentStep()
    {
        Name = nameof(AzureAISearchSettings);
    }
}
