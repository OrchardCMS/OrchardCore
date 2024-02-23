using OrchardCore.Deployment;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment
{
    /// <summary>
    /// Adds reset Elasticsearch index task to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class ElasticIndexResetDeploymentStep : DeploymentStep
    {
        public ElasticIndexResetDeploymentStep()
        {
            Name = "ElasticIndexReset";
        }

        public bool IncludeAll { get; set; } = true;

        public string[] Indices { get; set; }
    }
}
