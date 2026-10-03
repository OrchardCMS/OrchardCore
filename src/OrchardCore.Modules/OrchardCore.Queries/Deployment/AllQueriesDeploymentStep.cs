using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Queries.Deployment;

/// <summary>
/// Adds all queries to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllQueriesDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<AllQueriesDeploymentStep>("Content Management");

    public AllQueriesDeploymentStep()
    {
        Name = "AllQueries";
        Category = s_category;
    }
}
