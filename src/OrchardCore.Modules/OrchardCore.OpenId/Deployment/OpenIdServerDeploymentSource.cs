using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;

namespace OrchardCore.OpenId.Deployment
{
    public class OpenIdServerDeploymentSource : IDeploymentSource
    {
        private readonly IOpenIdServerService _openIdServerService;

        public OpenIdServerDeploymentSource(IOpenIdServerService openIdServerService)
        {
            _openIdServerService = openIdServerService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var openIdServerStep = step as OpenIdServerDeploymentStep;

            if (openIdServerStep == null)
            {
                return;
            }

            var serverSettings = await _openIdServerService
                .GetSettingsAsync();

            // Use nameof(OpenIdServerSettings) as name,
            // to match the recipe step.
            var obj = new JObject(
                new JProperty(
                    "name",
                    nameof(OpenIdServerSettings)));

            obj.Merge(JObject.FromObject(serverSettings));

            result.Steps.Add(obj);
        }
    }
}
