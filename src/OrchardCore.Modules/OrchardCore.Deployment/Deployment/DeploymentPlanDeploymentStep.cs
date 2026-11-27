using OrchardCore.Localization;

namespace OrchardCore.Deployment.Deployment;

/// <summary>
/// Adds deployment plans to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class DeploymentPlanDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "DeploymentPlan";

    public static LocalizationSource Category => DeploymentLocalizationSources.Deployment;

    public static LocalizationSource Title => DeploymentLocalizationSources.DeploymentPlans;

    public bool IncludeAll { get; set; } = true;

    public string[] DeploymentPlanNames { get; set; }
}
