using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Microsoft.Authentication.Services;

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
            var azureADStep = step as AzureADDeploymentStep;

            if (azureADStep == null)
            {
                return;
            }

            var settings = await _azureADService.GetSettingsAsync();

            result.Steps.Add(new JObject(
                new JProperty("name", "AzureAD"),
                new JProperty("AzureAD", JObject.FromObject(settings))
            ));
        }
    }
}
