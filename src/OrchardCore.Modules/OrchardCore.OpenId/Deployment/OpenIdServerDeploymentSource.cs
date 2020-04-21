using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.OpenId.Services;

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

            var serverSettings = await _openIdServerService.GetSettingsAsync();

            result.Steps.Add(new JObject(
                new JProperty("name", "OpenIdServer"),
                new JProperty("OpenIdServer", JObject.FromObject(serverSettings))
            ));
        }
    }
}
