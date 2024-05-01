using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.Deployment.Steps
{
    public class CustomFileDeploymentSource : IDeploymentSource
    {
        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            if (step is not CustomFileDeploymentStep customFile)
            {
                return;
            }

            if (result is FileDeploymentPlanResult fileResult)
            {
                await fileResult.FileBuilder.SetFileAsync(customFile.FileName, Encoding.UTF8.GetBytes(customFile.FileContent));
            }
        }
    }
}
