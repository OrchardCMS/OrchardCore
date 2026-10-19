using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Elasticsearch.Core.Deployment;

/// <summary>
/// Adds layers to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public sealed class ElasticsearchIndexDeploymentStep : DeploymentStep
{
    public ElasticsearchIndexDeploymentStep()
    {
        Name = "ElasticIndexSettings";
        Category = CommonLocalizationSources.Search;
        Title = ElasticsearchLocalizationSources.ElasticsearchSearchIndexes;
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
