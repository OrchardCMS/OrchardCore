using OrchardCore.Deployment;

namespace OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget;

public sealed class ExportContentToDeploymentTargetDeploymentStepDriver
    : DeploymentStepFieldsDriverBase<ExportContentToDeploymentTargetDeploymentStep>
{
    public ExportContentToDeploymentTargetDeploymentStepDriver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}
