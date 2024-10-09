using System.Text;

namespace OrchardCore.Deployment.Steps;

public class CustomFileDeploymentSource
    : DeploymentSourceBase<CustomFileDeploymentStep>
{
    protected override async Task ProcessAsync(CustomFileDeploymentStep step, DeploymentPlanResult result)
        => await result.FileBuilder.SetFileAsync(step.FileName, Encoding.UTF8.GetBytes(step.FileContent));
}
