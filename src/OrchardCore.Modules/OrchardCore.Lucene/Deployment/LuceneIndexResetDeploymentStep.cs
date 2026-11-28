using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Lucene.Deployment;

/// <summary>
/// Adds reset Lucene index task to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class LuceneIndexResetDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "LuceneIndexReset";

    public static LocalizationSource Category => CommonLocalizationSources.Search;

    public static LocalizationSource Title => LuceneLocalizationSources.ResetLuceneSearchIndices;

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
