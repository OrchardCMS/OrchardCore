using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Queries.Deployment;

/// <summary>
/// Adds all queries to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllQueriesDeploymentStep : DeploymentStep
{
    public override string Name => "AllQueries";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.ContentManagement;
}
