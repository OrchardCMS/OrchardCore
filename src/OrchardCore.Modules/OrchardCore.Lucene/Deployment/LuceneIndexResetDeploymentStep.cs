using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Lucene.Deployment;

/// <summary>
/// Adds reset Lucene index task to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class LuceneIndexResetDeploymentStep : DeploymentStep
{
    public override string Name => "LuceneIndexReset";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Search;

    [JsonIgnore]
    public override LocalizationSource Title => LuceneLocalizationSources.ResetLuceneSearchIndices;

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
