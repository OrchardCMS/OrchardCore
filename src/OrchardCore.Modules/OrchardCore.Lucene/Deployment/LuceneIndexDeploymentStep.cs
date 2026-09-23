using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Lucene.Deployment;

/// <summary>
/// Adds layers to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class LuceneIndexDeploymentStep : DeploymentStep
{
    public LuceneIndexDeploymentStep()
    {
        Name = "LuceneIndex";
        Category = LocalizedString.Create("Search");
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
