using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Microsoft.Authentication.Settings;

namespace OrchardCore.Microsoft.Authentication.Deployment
{
    public class AzureADDeploymentSource : IDeploymentSource
    {
        private readonly AzureADSettings _azureADSettings;

        public AzureADDeploymentSource(IOptions<AzureADSettings> azureADSettings)
        {
            _azureADSettings = azureADSettings.Value;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var azureADStep = step as AzureADDeploymentStep;

            if (azureADStep == null)
            {
                await Task.CompletedTask;
            }
            result.Steps.Add(new JObject(
                new JProperty("name", "AzureAD"),
                new JProperty("AzureAD", JObject.FromObject(_azureADSettings))
            ));
        }
    }
}
