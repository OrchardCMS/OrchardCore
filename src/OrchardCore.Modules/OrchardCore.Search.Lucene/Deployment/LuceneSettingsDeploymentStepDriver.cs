using OrchardCore.Deployment;

namespace OrchardCore.Search.Lucene.Deployment;

public sealed class LuceneSettingsDeploymentStepDriver
    : DeploymentStepFieldsDriverBase<LuceneSettingsDeploymentStep>
{
    public LuceneSettingsDeploymentStepDriver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}
