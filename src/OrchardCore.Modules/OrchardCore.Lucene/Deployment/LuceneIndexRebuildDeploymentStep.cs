using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Lucene.Deployment;

/// <summary>
/// Adds rebuild Lucene index task to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class LuceneIndexRebuildDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Search", typeof(LuceneIndexRebuildDeploymentStep));
    private static readonly LocalizationSource s_title = new("Rebuild Lucene Search Indices", typeof(LuceneIndexRebuildDeploymentStep));

    public LuceneIndexRebuildDeploymentStep()
    {
        Name = "LuceneIndexRebuild";
        Category = s_category;
        Title = s_title;
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
