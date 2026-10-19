using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Lucene.Deployment;

/// <summary>
/// Adds rebuild Lucene index task to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class LuceneIndexRebuildDeploymentStep : DeploymentStep
{
    public LuceneIndexRebuildDeploymentStep()
    {
        Name = "LuceneIndexRebuild";
        Category = CommonLocalizationSources.Search;
        Title = LuceneLocalizationSources.RebuildLuceneSearchIndices;
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
