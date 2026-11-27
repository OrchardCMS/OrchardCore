using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Elasticsearch.Core.Deployment;

/// <summary>
/// Adds reset Elasticsearch index task to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public sealed class ElasticsearchIndexResetDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "ElasticIndexReset";

    public static LocalizationSource Category => CommonLocalizationSources.Search;

    public static LocalizationSource Title => ElasticsearchLocalizationSources.ResetElasticsearchIndices;

    public bool IncludeAll { get; set; } = true;

    public string[] Indices { get; set; }
}
