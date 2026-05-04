using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Elasticsearch.Core.Deployment;

/// <summary>
/// Adds reset Elasticsearch index task to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public sealed class ElasticsearchIndexResetDeploymentStep : DeploymentStep
{
    public ElasticsearchIndexResetDeploymentStep()
    {
        Name = "ElasticIndexReset";
    }

    public ElasticsearchIndexResetDeploymentStep(IStringLocalizer<ElasticsearchIndexResetDeploymentStep> S)
        : this()
    {
        Category = S["Search"];
    }

    public bool IncludeAll { get; set; } = true;

    public string[] Indices { get; set; }
}
