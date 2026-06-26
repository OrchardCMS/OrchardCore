using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Lucene.Deployment;

/// <summary>
/// Adds reset Lucene index task to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class LuceneIndexResetDeploymentStep : DeploymentStep
{
    public LuceneIndexResetDeploymentStep()
    {
        Name = "LuceneIndexReset";
    }

    public LuceneIndexResetDeploymentStep(IStringLocalizer<LuceneIndexResetDeploymentStep> S)
        : this()
    {
        Category = S["Search"];
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
