using OrchardCore.Localization;

namespace OrchardCore.Deployment.Deployment;

/// <summary>
/// Adds deployment plans to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class DeploymentPlanDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<DeploymentPlanDeploymentStep>("Deployment");
    private static readonly LocalizationSource s_title = LocalizationSource.Create<DeploymentPlanDeploymentStep>("Deployment Plans");

    public DeploymentPlanDeploymentStep()
    {
        Name = "DeploymentPlan";
        Category = s_category;
        Title = s_title;
    }

    public bool IncludeAll { get; set; } = true;

    public string[] DeploymentPlanNames { get; set; }
}
