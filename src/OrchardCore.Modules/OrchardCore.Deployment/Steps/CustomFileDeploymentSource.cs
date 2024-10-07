using System.Text;

namespace OrchardCore.Deployment.Steps;

public class CustomFileDeploymentSource
    : DeploymentSourceBase<CustomFileDeploymentStep>
{
    public override async Task ProcessDeploymentStepAsync(DeploymentPlanResult result)
        => await result.FileBuilder.SetFileAsync(DeploymentStep.FileName, Encoding.UTF8.GetBytes(DeploymentStep.FileContent));
}
