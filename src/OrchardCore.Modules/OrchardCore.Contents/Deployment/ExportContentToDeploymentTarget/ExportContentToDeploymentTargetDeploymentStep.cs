using OrchardCore.Deployment;

namespace OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget
{
    /// <summary>
    /// Adds content selected with export content to deployment plan target feature to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class ExportContentToDeploymentTargetDeploymentStep : DeploymentStep
    {
        public ExportContentToDeploymentTargetDeploymentStep()
        {
            Name = nameof(ExportContentToDeploymentTargetDeploymentStep);
        }
    }
}
