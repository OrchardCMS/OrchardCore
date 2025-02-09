using OrchardCore.Deployment;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment;

/// <summary>
/// Adds layers to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public sealed class ElasticSettingsDeploymentStep : DeploymentStep
{
    public ElasticSettingsDeploymentStep()
    {
        Name = "ElasticSettings";
    }
}
