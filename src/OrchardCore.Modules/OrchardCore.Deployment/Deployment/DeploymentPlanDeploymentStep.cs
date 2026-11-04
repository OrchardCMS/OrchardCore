using System.Text.Json.Serialization;
using OrchardCore.Localization;

namespace OrchardCore.Deployment.Deployment;

/// <summary>
/// Adds deployment plans to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class DeploymentPlanDeploymentStep : DeploymentStep
{
    public override string Name => "DeploymentPlan";

    [JsonIgnore]
    public override LocalizationSource Category => DeploymentLocalizationSources.Deployment;

    [JsonIgnore]
    public override LocalizationSource Title => DeploymentLocalizationSources.DeploymentPlans;

    public bool IncludeAll { get; set; } = true;

    public string[] DeploymentPlanNames { get; set; }
}
