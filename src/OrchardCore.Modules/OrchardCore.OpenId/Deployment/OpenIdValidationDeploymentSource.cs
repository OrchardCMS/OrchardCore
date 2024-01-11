using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;

namespace OrchardCore.OpenId.Deployment
{
    public class OpenIdValidationDeploymentSource : IDeploymentSource
    {
        private readonly IOpenIdValidationService _openIdValidationService;

        public OpenIdValidationDeploymentSource(IOpenIdValidationService openIdValidationService)
        {
            _openIdValidationService = openIdValidationService;
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
            var jObject = new JObject(new JProperty("name", nameof(OpenIdValidationSettings)));

            // Merge settings as the recipe step doesn't use a child property.
            jObject.Merge(JObject.FromObject(validationSettings));

            result.Steps.Add(jObject);
        }
    }
}
