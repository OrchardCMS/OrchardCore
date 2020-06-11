namespace OrchardCore.Deployment.ViewModels
{
    public class DeploymentPlanDeploymentStepViewModel
    {
        public bool IncludeAll { get; set; } = true;
        public string[] DeploymentPlanNames { get; set; }
        public string[] AllDeploymentPlanNames { get; set; }
    }
}
