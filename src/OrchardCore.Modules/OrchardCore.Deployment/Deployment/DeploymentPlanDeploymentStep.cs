using Microsoft.Extensions.Localization;

namespace OrchardCore.Deployment.Deployment;

/// <summary>
/// Adds deployment plans to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class DeploymentPlanDeploymentStep : DeploymentStep
{
    public DeploymentPlanDeploymentStep()
    {
        Name = "DeploymentPlan";
    }

    public DeploymentPlanDeploymentStep(IStringLocalizer<DeploymentPlanDeploymentStep> S)
        : this()
    {
        Category = S["Deployment"];
    }

    public bool IncludeAll { get; set; } = true;

    public string[] DeploymentPlanNames { get; set; }
}
