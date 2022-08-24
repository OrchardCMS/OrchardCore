using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Entities;
using OrchardCore.Search.Model;
using OrchardCore.Settings;

namespace OrchardCore.Search.Deployment
{
    public class SearchSettingsDeploymentSource : IDeploymentSource
    {
        private readonly ISiteService _site;

        public SearchSettingsDeploymentSource(ISiteService site)
        {
            _site = site;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var searchSettingsStep = step as SearchSettingsDeploymentStep;

            if (searchSettingsStep == null)
            {
                return;
            }

            var settings = await _site.GetSiteSettingsAsync();
            var searchSettings = settings.As<SearchSettings>();

            result.Steps.Add(new JObject(
                new JProperty("name", "Settings"),
                new JProperty("SearchSettings", JObject.FromObject(searchSettings))
            ));
        }
    }
}
