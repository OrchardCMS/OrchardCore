using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Roles.Deployment;

/// <summary>
/// Adds roles to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllRolesDeploymentStep : DeploymentStep
{
    public override string Name => "AllRoles";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Security;

    /// <summary>
    /// Gets or sets the exact names of the roles to export. All of them are exported when it is <see langword="null"/> or empty.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[] Names { get; set; }
}
