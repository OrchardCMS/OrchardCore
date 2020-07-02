using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;

namespace OrchardCore.Settings.Deployment
{
    public class GenericSiteSettingsDeploymentSource : IDeploymentSource
    {
        private readonly ISiteService _siteService;

        public GenericSiteSettingsDeploymentSource(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var settingsStep = step as GenericSiteSettingsDeploymentStep;
            if (settingsStep == null)
            {
                return;
            }

            var site = await _siteService.GetSiteSettingsAsync();

            var data = new JObject(new JProperty("name", "Settings"));
            JToken value;

            if (site.Properties.TryGetValue(step.Name, out value))
            {
                data.Add(new JProperty(step.Name, value));
            }

            result.Steps.Add(data);

            return;
        }
    }
}
