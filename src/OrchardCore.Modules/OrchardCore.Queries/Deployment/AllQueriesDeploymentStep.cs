using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Queries.Deployment;

/// <summary>
/// Adds all queries to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllQueriesDeploymentStep : DeploymentStep
{
    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.ContentManagement;

    public AllQueriesDeploymentStep()
    {
        Name = "AllQueries";
    }
}
