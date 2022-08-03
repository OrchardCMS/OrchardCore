using OrchardCore.Deployment;

namespace OrchardCore.Search.Elasticsearch.Deployment
{
    /// <summary>
    /// Adds layers to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class ElasticsearchSettingsDeploymentStep : DeploymentStep
    {
        public ElasticsearchSettingsDeploymentStep()
        {
            Name = "ElasticsearchSettings";
        }
    }
}
