using System.Text.Json;
using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;

namespace OrchardCore.Microsoft.Authentication.Deployment
{
    public class AzureADDeploymentSource : IDeploymentSource
    {
        private readonly IAzureADService _azureADService;

        public AzureADDeploymentSource(IAzureADService azureADService)
        {
            _azureADService = azureADService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            if (step is not AzureADDeploymentStep)
            {
                return;
            }

            var settings = await _azureADService.GetSettingsAsync();

            var stepJson = JsonSerializer.SerializeToNode(settings)!.AsObject();
            stepJson["name"] = nameof(AzureADSettings);

            result.Steps.Add(stepJson);
        }
    }
}
