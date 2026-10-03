using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Lucene.Deployment;

/// <summary>
/// Adds reset Lucene index task to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class LuceneIndexResetDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Search", typeof(LuceneIndexResetDeploymentStep));
    private static readonly LocalizationSource s_title = new("Reset Lucene Search Indices", typeof(LuceneIndexResetDeploymentStep));

    public LuceneIndexResetDeploymentStep()
    {
        Name = "LuceneIndexReset";
        Category = s_category;
        Title = s_title;
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
