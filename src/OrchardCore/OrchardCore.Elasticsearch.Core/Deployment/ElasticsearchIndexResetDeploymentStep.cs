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
        Category = LocalizedString.Create("Search");
        Title = LocalizedString.Create("Reset Elasticsearch Indices");
    }

    public bool IncludeAll { get; set; } = true;

    public string[] Indices { get; set; }
}
