using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Elasticsearch.Core.Deployment;

/// <summary>
/// Adds layers to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public sealed class ElasticsearchIndexDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<ElasticsearchIndexDeploymentStep>("Search");
    private static readonly LocalizationSource s_title = LocalizationSource.Create<ElasticsearchIndexDeploymentStep>("Elasticsearch Search Indexes");

    public ElasticsearchIndexDeploymentStep()
    {
        Name = "ElasticIndexSettings";
        Category = s_category;
        Title = s_title;
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
