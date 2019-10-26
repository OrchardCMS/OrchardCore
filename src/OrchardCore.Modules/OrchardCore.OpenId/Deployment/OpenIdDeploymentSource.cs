using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.OpenId.Services;

namespace OrchardCore.OpenId.Deployment
{
    public class OpenIdDeploymentSource : IDeploymentSource
    {
        private readonly IOpenIdServerService _openIdServerService;

        public OpenIdDeploymentSource(IOpenIdServerService openIdServerService)
        {
            _openIdServerService = openIdServerService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var OpenIdState = step as OpenIdDeploymentStep;

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
