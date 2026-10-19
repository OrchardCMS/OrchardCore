using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Elasticsearch.Core.Deployment;

/// <summary>
/// Adds rebuild Elasticsearch index task to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public sealed class ElasticsearchIndexRebuildDeploymentStep : DeploymentStep
{
    public ElasticsearchIndexRebuildDeploymentStep()
    {
        Name = "ElasticIndexRebuild";
        Category = CommonLocalizationSources.Search;
        Title = ElasticsearchLocalizationSources.RebuildElasticsearchIndices;
    }

    public bool IncludeAll { get; set; } = true;

    public string[] Indices { get; set; }
}
