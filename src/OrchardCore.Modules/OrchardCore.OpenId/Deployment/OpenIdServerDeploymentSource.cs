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
            var OpenIdState = step as OpenIdServerDeploymentStep;

            if (OpenIdState == null)
            {
                return;
            }

            var serverSettings = await _openIdServerService.GetSettingsAsync();

            result.Steps.Add(new JObject(
                new JProperty("name", "OpenId"),
                new JProperty("OpenId", JObject.FromObject(serverSettings))
            ));
        }
    }
}
