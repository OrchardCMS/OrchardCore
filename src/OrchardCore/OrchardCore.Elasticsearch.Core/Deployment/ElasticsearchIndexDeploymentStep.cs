using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Elasticsearch.Core.Deployment;

/// <summary>
/// Adds layers to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public sealed class ElasticsearchIndexDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "ElasticIndexSettings";

    public static LocalizationSource Category => CommonLocalizationSources.Search;

    public static LocalizationSource Title => ElasticsearchLocalizationSources.ElasticsearchSearchIndexes;

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
