using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.OpenId.Services;

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

            result.Steps.Add(new JObject(
                new JProperty("name", "OpenIdValidation"),
                new JProperty("OpenIdValidation", JObject.FromObject(validationSettings))
            ));
        }
    }
}
