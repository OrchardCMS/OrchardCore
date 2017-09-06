using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.Deployment.Steps
{
    public class CustomFileDeploymentSource : IDeploymentSource
    {
        public Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var customFile = step as CustomFileDeploymentStep;

            if (customFile == null)
            {
                return Task.CompletedTask;
            }

            return result.FileBuilder.SetFileAsync(customFile.FileName, Encoding.UTF8.GetBytes(customFile.FileContent));
        }
    }
}
