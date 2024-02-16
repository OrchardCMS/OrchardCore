using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Search.Models;
using OrchardCore.Settings;

namespace OrchardCore.Search.Deployment
{
    public class SearchSettingsDeploymentSource : IDeploymentSource
    {
        private readonly ISiteService _site;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public SearchSettingsDeploymentSource(
            ISiteService site,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _site = site;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
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

            result.Steps.Add(new JsonObject
            {
                ["name"] = "Settings",
                ["SearchSettings"] = JObject.FromObject(searchSettings, _jsonSerializerOptions),
            });
        }
    }
}
