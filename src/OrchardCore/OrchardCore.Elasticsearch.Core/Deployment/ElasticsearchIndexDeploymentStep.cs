using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Elasticsearch.Core.Deployment;

/// <summary>
/// Adds layers to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public sealed class ElasticsearchIndexDeploymentStep : DeploymentStep
{
    public ElasticsearchIndexDeploymentStep()
    {
        Name = "ElasticIndexSettings";
    }

    public ElasticsearchIndexDeploymentStep(IStringLocalizer<ElasticsearchIndexDeploymentStep> S)
        : this()
    {
        Category = S["Search"];
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
