using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Lucene.Deployment;

/// <summary>
/// Adds layers to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class LuceneIndexDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Search", typeof(LuceneIndexDeploymentStep));
    private static readonly LocalizationSource s_title = new("Lucene Search Indexes", typeof(LuceneIndexDeploymentStep));

    public LuceneIndexDeploymentStep()
    {
        Name = "LuceneIndex";
        Category = s_category;
        Title = s_title;
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
