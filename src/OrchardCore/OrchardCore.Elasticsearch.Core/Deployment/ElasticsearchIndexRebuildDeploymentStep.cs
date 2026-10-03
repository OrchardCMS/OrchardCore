using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Elasticsearch.Core.Deployment;

/// <summary>
/// Adds rebuild Elasticsearch index task to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public sealed class ElasticsearchIndexRebuildDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Search", typeof(ElasticsearchIndexRebuildDeploymentStep));
    private static readonly LocalizationSource s_title = new("Rebuild Elasticsearch Indices", typeof(ElasticsearchIndexRebuildDeploymentStep));

    public ElasticsearchIndexRebuildDeploymentStep()
    {
        Name = "ElasticIndexRebuild";
        Category = s_category;
        Title = s_title;
    }

    public bool IncludeAll { get; set; } = true;

    public string[] Indices { get; set; }
}
