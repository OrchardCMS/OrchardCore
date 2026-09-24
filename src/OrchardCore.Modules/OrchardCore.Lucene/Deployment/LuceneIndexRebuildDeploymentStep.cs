using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Lucene.Deployment;

/// <summary>
/// Adds rebuild Lucene index task to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class LuceneIndexRebuildDeploymentStep : DeploymentStep
{
    public LuceneIndexRebuildDeploymentStep()
    {
        Name = "LuceneIndexRebuild";
        Category = LocalizedString.Create("Search");
        Title = LocalizedString.Create("Rebuild Lucene Search Indices");
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
