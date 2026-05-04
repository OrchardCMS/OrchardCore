using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Elasticsearch.Core.Deployment;

/// <summary>
/// Adds rebuild Elasticsearch index task to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public sealed class ElasticsearchIndexRebuildDeploymentStep : DeploymentStep
{
    public ElasticsearchIndexRebuildDeploymentStep()
    {
        Name = "ElasticIndexRebuild";
    }

    public ElasticsearchIndexRebuildDeploymentStep(IStringLocalizer<ElasticsearchIndexRebuildDeploymentStep> S)
        : this()
    {
        Category = S["Search"];
    }

    public bool IncludeAll { get; set; } = true;

    public string[] Indices { get; set; }
}
