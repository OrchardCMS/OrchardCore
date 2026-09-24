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
        Category = LocalizedString.Create("Search");
        Title = LocalizedString.Create("Elasticsearch Search Indexes");
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
