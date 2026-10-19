using System.Text.Json.Serialization;
using OrchardCore.Localization;

namespace OrchardCore.Deployment.Deployment;

/// <summary>
/// Adds deployment plans to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class DeploymentPlanDeploymentStep : DeploymentStep
{
    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Deployment;

    [JsonIgnore]
    public override LocalizationSource Title => DeploymentLocalizationSources.DeploymentPlans;

    public DeploymentPlanDeploymentStep()
    {
        Name = "DeploymentPlan";
    }

    public bool IncludeAll { get; set; } = true;

    public string[] DeploymentPlanNames { get; set; }
}
