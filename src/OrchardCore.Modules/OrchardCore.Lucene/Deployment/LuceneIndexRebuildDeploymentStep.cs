using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Lucene.Deployment;

/// <summary>
/// Adds rebuild Lucene index task to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class LuceneIndexRebuildDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<LuceneIndexRebuildDeploymentStep>("Search");
    private static readonly LocalizationSource s_title = LocalizationSource.Create<LuceneIndexRebuildDeploymentStep>("Rebuild Lucene Search Indices");

    public LuceneIndexRebuildDeploymentStep()
    {
        Name = "LuceneIndexRebuild";
        Category = s_category;
        Title = s_title;
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
