using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Queries.Deployment;

/// <summary>
/// Adds all queries to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllQueriesDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Content Management", typeof(AllQueriesDeploymentStep));

    public AllQueriesDeploymentStep()
    {
        Name = "AllQueries";
        Category = s_category;
    }
}
