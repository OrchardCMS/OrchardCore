using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Entities;

namespace OrchardCore.Settings.Deployment
{
    public class AllSiteSettingsDeploymentSource : IDeploymentSource
    {
        private readonly ISiteService _siteService;

        public AllSiteSettingsDeploymentSource(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var settingsStep = step as AllSiteSettingsDeploymentStep;
            if (settingsStep == null)
            {
                return;
            }

            var site = await _siteService.GetSiteSettingsAsync();

            var data = new JObject(new JProperty("name", "Settings"));

            foreach (var settingName in settingsStep.Settings)
            {
                JToken value;
                if (site.Properties.TryGetValue(settingName, out value))
                {
                    data.Add(new JProperty(settingName, value));
                }
            }

            result.Steps.Add(data);

            return;
        }
    }
}
