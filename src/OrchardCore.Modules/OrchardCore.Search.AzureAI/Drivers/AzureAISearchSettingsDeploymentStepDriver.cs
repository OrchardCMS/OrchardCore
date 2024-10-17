using OrchardCore.Deployment;
using OrchardCore.Search.AzureAI.Deployment;

namespace OrchardCore.Search.AzureAI.Drivers;

public sealed class AzureAISearchSettingsDeploymentStepDriver
    : DeploymentStepFieldsDriverBase<AzureAISearchSettingsDeploymentStep>
{
    public AzureAISearchSettingsDeploymentStepDriver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}
