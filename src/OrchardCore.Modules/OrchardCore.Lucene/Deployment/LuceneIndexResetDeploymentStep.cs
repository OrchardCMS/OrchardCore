using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Lucene.Deployment;

/// <summary>
/// Adds reset Lucene index task to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class LuceneIndexResetDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<LuceneIndexResetDeploymentStep>("Search");
    private static readonly LocalizationSource s_title = LocalizationSource.Create<LuceneIndexResetDeploymentStep>("Reset Lucene Search Indices");

    public LuceneIndexResetDeploymentStep()
    {
        Name = "LuceneIndexReset";
        Category = s_category;
        Title = s_title;
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
