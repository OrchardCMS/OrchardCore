using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.Search.Models;
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
            if (step is not SearchSettingsDeploymentStep)
            {
                return;
            }

            var settings = await _site.GetSiteSettingsAsync();
            var searchSettings = settings.As<SearchSettings>();

            result.Steps.Add(new JsonObject
            {
                ["name"] = "Settings",
                ["SearchSettings"] = JObject.FromObject(searchSettings),
            });
        }
    }
}
