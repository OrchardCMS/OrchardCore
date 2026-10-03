using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Elasticsearch.Core.Deployment;

/// <summary>
/// Adds layers to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public sealed class ElasticsearchIndexDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Search", typeof(ElasticsearchIndexDeploymentStep));
    private static readonly LocalizationSource s_title = new("Elasticsearch Search Indexes", typeof(ElasticsearchIndexDeploymentStep));

    public ElasticsearchIndexDeploymentStep()
    {
        Name = "ElasticIndexSettings";
        Category = s_category;
        Title = s_title;
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
