using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;

namespace OrchardCore.OpenId.Deployment
{
    public class OpenIdValidationDeploymentSource : IDeploymentSource
    {
        private readonly IOpenIdValidationService _openIdValidationService;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public OpenIdValidationDeploymentSource(
            IOpenIdValidationService openIdValidationService,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _openIdValidationService = openIdValidationService;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var openIdValidationStep = step as OpenIdValidationDeploymentStep;

            if (openIdValidationStep == null)
            {
                return;
            }

            var validationSettings = await _openIdValidationService.GetSettingsAsync();

            // The 'name' property should match the related recipe step name.
            var jObject = new JsonObject { ["name"] = nameof(OpenIdValidationSettings) };

            // Merge settings as the recipe step doesn't use a child property.
            jObject.Merge(JObject.FromObject(validationSettings, _jsonSerializerOptions));

            result.Steps.Add(jObject);
        }
    }
}
