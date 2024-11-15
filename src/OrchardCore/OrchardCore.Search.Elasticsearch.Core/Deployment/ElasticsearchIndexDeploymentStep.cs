using OrchardCore.Deployment;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment;

/// <summary>
/// Adds layers to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public sealed class ElasticsearchIndexDeploymentStep : DeploymentStep
{
    public ElasticsearchIndexDeploymentStep()
    {
        Name = "ElasticIndexSettings";
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
