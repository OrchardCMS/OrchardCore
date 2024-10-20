using OrchardCore.Deployment;

namespace OrchardCore.Search.Deployment;

public sealed class SearchSettingsDeploymentStepDriver
    : DeploymentStepFieldsDriverBase<SearchSettingsDeploymentStep>
{
    public SearchSettingsDeploymentStepDriver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}
