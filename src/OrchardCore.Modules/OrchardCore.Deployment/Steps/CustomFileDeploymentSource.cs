using System.Text;

namespace OrchardCore.Deployment.Steps;

public class CustomFileDeploymentSource
    : DeploymentSourceBase<CustomFileDeploymentStep>
{
    protected override Task ProcessAsync(CustomFileDeploymentStep step, DeploymentPlanResult result)
        => result.FileBuilder.SetFileAsync(step.FileName, Encoding.UTF8.GetBytes(step.FileContent));
}
