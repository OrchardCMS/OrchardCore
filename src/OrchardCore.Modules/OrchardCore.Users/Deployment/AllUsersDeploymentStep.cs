using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Users.Deployment;

/// <summary>
/// Adds users to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllUsersDeploymentStep : DeploymentStep
{
    public override string Name => "AllUsers";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Security;
}
