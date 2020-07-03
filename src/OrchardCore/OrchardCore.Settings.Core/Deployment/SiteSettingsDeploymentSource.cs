using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;

namespace OrchardCore.Settings.Deployment
{
    public class SiteSettingsDeploymentSource<TModel> : IDeploymentSource where TModel : class
    {
        private readonly ISiteService _siteService;

        public SiteSettingsDeploymentSource(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var settingsStep = step as SiteSettingsDeploymentStep<TModel>;
            if (settingsStep == null)
            {
                return;
            }

            var site = await _siteService.GetSiteSettingsAsync();

            var data = new JObject(new JProperty("name", "Settings"));
            JToken value;

            var name = typeof(TModel).Name;
            if (site.Properties.TryGetValue(name, out value))
            {
                data.Add(new JProperty(name, value));
            }

            result.Steps.Add(data);

            return;
        }
    }
}
