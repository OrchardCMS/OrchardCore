using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Google.Analytics.Services;

namespace OrchardCore.Google.Analytics.Deployment
{
    public class GoogleAnalyticsDeploymentSource : IDeploymentSource
    {
        private readonly IGoogleAnalyticsService _googleAnalyticsService;

        public GoogleAnalyticsDeploymentSource(IGoogleAnalyticsService googleAnalyticsService)
        {
            _googleAnalyticsService = googleAnalyticsService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var googleAnalyticsStep = step as GoogleAnalyticsDeploymentStep;

            if (googleAnalyticsStep == null)
            {
                return;
            }

            var settings = await _googleAnalyticsService.GetSettingsAsync();

            result.Steps.Add(new JObject(
                new JProperty("name", "GoogleAnalytics"),
                new JProperty("GoogleAnalytics", JObject.FromObject(settings))
            ));
        }
    }
}
