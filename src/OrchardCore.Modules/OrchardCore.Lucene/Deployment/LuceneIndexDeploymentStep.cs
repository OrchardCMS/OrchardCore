using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Lucene.Deployment;

/// <summary>
/// Adds layers to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class LuceneIndexDeploymentStep : DeploymentStep
{
    public LuceneIndexDeploymentStep()
    {
        Name = "LuceneIndex";
        Category = CommonLocalizationSources.Search;
        Title = LuceneLocalizationSources.LuceneSearchIndexes;
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
