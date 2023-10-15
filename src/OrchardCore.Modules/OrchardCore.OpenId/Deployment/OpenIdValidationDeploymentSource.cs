using System.Text.Json;
using System.Threading.Tasks;
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
            var stepJson = JsonSerializer.SerializeToNode(validationSettings)!.AsObject();
            stepJson["name"] = nameof(OpenIdValidationSettings);

            result.Steps.Add(stepJson);
        }
    }
}
