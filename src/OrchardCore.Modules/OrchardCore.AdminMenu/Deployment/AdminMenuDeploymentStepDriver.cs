using OrchardCore.Deployment;

namespace OrchardCore.AdminMenu.Deployment;

public sealed class AdminMenuDeploymentStepDriver
    : DeploymentStepFieldsDriverBase<AdminMenuDeploymentStep>
{
    public AdminMenuDeploymentStepDriver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}
