using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;

namespace OrchardCore.Microsoft.Authentication.Deployment
{
    public class AzureADDeploymentSource : IDeploymentSource
    {
        private readonly IAzureADService _azureADService;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public AzureADDeploymentSource(
            IAzureADService azureADService,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _azureADService = azureADService;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var azureADStep = step as AzureADDeploymentStep;

            if (azureADStep == null)
            {
                return;
            }

            var settings = await _azureADService.GetSettingsAsync();

            var obj = new JsonObject { ["name"] = nameof(AzureADSettings) };

            obj.Merge(JObject.FromObject(settings, _jsonSerializerOptions));

            result.Steps.Add(obj);
        }
    }
}
