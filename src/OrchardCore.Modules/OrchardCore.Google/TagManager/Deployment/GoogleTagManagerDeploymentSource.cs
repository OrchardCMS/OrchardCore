using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Google.TagManager.Services;

namespace OrchardCore.Google.TagManager.Deployment
{
    public class GoogleTagManagerDeploymentSource : IDeploymentSource
    {
        private readonly IGoogleTagManagerService _googleTagManagerService;

        public GoogleTagManagerDeploymentSource(IGoogleTagManagerService googleTagManagerService)
        {
            _googleTagManagerService = googleTagManagerService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var googleTagManagerStep = step as GoogleTagManagerDeploymentStep;

            if (googleTagManagerStep == null)
            {
                return;
            }

            var settings = await _googleTagManagerService.GetSettingsAsync();

            result.Steps.Add(new JObject(
                new JProperty("name", "GoogleTagManager"),
                new JProperty("GoogleTagManager", JObject.FromObject(settings))
            ));
        }
    }
}
