using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Lucene.Deployment;

/// <summary>
/// Adds rebuild Lucene index task to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class LuceneIndexRebuildDeploymentStep : DeploymentStep
{
    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Search;

    [JsonIgnore]
    public override LocalizationSource Title => LuceneLocalizationSources.RebuildLuceneSearchIndices;

    public LuceneIndexRebuildDeploymentStep()
    {
        Name = "LuceneIndexRebuild";
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
