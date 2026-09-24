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
        Category = LocalizedString.Create("Search");
        Title = LocalizedString.Create("Rebuild Elasticsearch Indices");
    }

    public bool IncludeAll { get; set; } = true;

    public string[] Indices { get; set; }
}
