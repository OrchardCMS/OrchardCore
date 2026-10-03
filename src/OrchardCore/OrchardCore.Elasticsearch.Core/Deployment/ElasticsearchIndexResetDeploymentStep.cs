using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Elasticsearch.Core.Deployment;

/// <summary>
/// Adds reset Elasticsearch index task to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public sealed class ElasticsearchIndexResetDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Search", typeof(ElasticsearchIndexResetDeploymentStep));
    private static readonly LocalizationSource s_title = new("Reset Elasticsearch Indices", typeof(ElasticsearchIndexResetDeploymentStep));

    public ElasticsearchIndexResetDeploymentStep()
    {
        Name = "ElasticIndexReset";
        Category = s_category;
        Title = s_title;
    }

    public bool IncludeAll { get; set; } = true;

    public string[] Indices { get; set; }
}
