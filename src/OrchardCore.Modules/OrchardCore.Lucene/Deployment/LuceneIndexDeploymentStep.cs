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
    }

    public LuceneIndexDeploymentStep(IStringLocalizer<LuceneIndexDeploymentStep> S)
        : this()
    {
        Category = S["Search"];
        DisplayName = S["Lucene Search Indexes"];
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
