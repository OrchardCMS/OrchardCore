using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Elasticsearch.Core.Deployment;

/// <summary>
/// Adds reset Elasticsearch index task to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public sealed class ElasticsearchIndexResetDeploymentStep : DeploymentStep
{
    public override string Name => "ElasticIndexReset";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Search;

    [JsonIgnore]
    public override LocalizationSource Title => ElasticsearchLocalizationSources.ResetElasticsearchIndices;

    public bool IncludeAll { get; set; } = true;

    public string[] Indices { get; set; }
}
