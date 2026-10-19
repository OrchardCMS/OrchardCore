using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Lucene.Deployment;

/// <summary>
/// Adds layers to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class LuceneIndexDeploymentStep : DeploymentStep
{
    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Search;

    [JsonIgnore]
    public override LocalizationSource Title => LuceneLocalizationSources.LuceneSearchIndexes;

    public LuceneIndexDeploymentStep()
    {
        Name = "LuceneIndex";
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
