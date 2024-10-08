using System.Text;

namespace OrchardCore.Deployment.Steps;

public class CustomFileDeploymentSource
    : DeploymentSourceBase<CustomFileDeploymentStep>
{
    protected override async Task ProcessAsync(DeploymentStep step, DeploymentPlanResult result)
        => await result.FileBuilder.SetFileAsync(DeploymentStep.FileName, Encoding.UTF8.GetBytes(DeploymentStep.FileContent));
}
