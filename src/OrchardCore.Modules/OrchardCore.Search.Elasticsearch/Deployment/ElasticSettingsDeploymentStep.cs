using OrchardCore.Deployment;

namespace OrchardCore.Search.Elasticsearch.Deployment
{
    /// <summary>
    /// Adds layers to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class ElasticSettingsDeploymentStep : DeploymentStep
    {
        public ElasticSettingsDeploymentStep()
        {
            Name = "ElasticsearchSettings";
        }
    }
}
