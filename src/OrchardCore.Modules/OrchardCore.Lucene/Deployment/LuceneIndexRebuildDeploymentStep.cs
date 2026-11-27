using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Lucene.Deployment;

/// <summary>
/// Adds rebuild Lucene index task to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class LuceneIndexRebuildDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "LuceneIndexRebuild";

    public static LocalizationSource Category => CommonLocalizationSources.Search;

    public static LocalizationSource Title => LuceneLocalizationSources.RebuildLuceneSearchIndices;

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
