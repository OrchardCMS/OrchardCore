using OrchardCore.Deployment;

namespace OrchardCore.Workflows.Deployment;

public sealed class AllWorkflowTypeDeploymentStepDriver
    : DeploymentStepFieldsDriverBase<AllWorkflowTypeDeploymentStep>
{
    public AllWorkflowTypeDeploymentStepDriver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}
