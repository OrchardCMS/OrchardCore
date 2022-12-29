using OrchardCore.Deployment;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment
{
    /// <summary>
    /// Adds rebuild Elasticsearch index task to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class ElasticIndexRebuildDeploymentStep : DeploymentStep
    {
        public ElasticIndexRebuildDeploymentStep()
        {
            Name = "ElasticIndexRebuild";
        }

        public bool IncludeAll { get; set; } = true;

        public string[] Indices { get; set; }
    }
}
