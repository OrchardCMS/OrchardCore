using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Elasticsearch.Core.Deployment;

/// <summary>
/// Adds layers to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public sealed class ElasticsearchIndexDeploymentStep : DeploymentStep
{
    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Search;

    [JsonIgnore]
    public override LocalizationSource Title => ElasticsearchLocalizationSources.ElasticsearchSearchIndexes;

    public ElasticsearchIndexDeploymentStep()
    {
        Name = "ElasticIndexSettings";
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
