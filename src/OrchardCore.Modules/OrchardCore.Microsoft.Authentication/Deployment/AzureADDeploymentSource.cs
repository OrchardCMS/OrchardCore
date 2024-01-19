using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
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
            var azureADStep = step as AzureADDeploymentStep;

            if (azureADStep == null)
            {
                return;
            }

            var settings = await _azureADService.GetSettingsAsync();

            var obj = new JObject(new JProperty("name", nameof(AzureADSettings)));

            obj.Merge(JObject.FromObject(settings));

            result.Steps.Add(obj);
        }
    }
}
