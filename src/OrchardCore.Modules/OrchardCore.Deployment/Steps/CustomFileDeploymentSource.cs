using System.Text;

namespace OrchardCore.Deployment.Steps;

public class CustomFileDeploymentSource : IDeploymentSource
{
    public Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        if (step is not CustomFileDeploymentStep customFile)
        {
            return Task.CompletedTask;
        }

        return result.FileBuilder.SetFileAsync(customFile.FileName, Encoding.UTF8.GetBytes(customFile.FileContent));
    }
}
