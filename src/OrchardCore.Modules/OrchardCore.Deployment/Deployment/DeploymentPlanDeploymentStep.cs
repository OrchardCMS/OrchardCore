using OrchardCore.Localization;

namespace OrchardCore.Deployment.Deployment;

/// <summary>
/// Adds deployment plans to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class DeploymentPlanDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Deployment", typeof(DeploymentPlanDeploymentStep));
    private static readonly LocalizationSource s_title = new("Deployment Plans", typeof(DeploymentPlanDeploymentStep));

    public DeploymentPlanDeploymentStep()
    {
        Name = "DeploymentPlan";
        Category = s_category;
        Title = s_title;
    }

    public bool IncludeAll { get; set; } = true;

    public string[] DeploymentPlanNames { get; set; }
}
