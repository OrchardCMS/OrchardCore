using OrchardCore.Deployment;
using OrchardCore.Search.Elasticsearch.Core.Deployment;

namespace OrchardCore.Search.Elasticsearch.Drivers;

public sealed class ElasticSettingsDeploymentStepDriver
    : DeploymentStepFieldsDriverBase<ElasticSettingsDeploymentStep>
{
    public ElasticSettingsDeploymentStepDriver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}
