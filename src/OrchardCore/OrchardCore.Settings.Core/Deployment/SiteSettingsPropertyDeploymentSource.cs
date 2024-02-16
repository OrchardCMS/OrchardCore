using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;

namespace OrchardCore.Settings.Deployment
{
    public class SiteSettingsPropertyDeploymentSource<TModel> : IDeploymentSource where TModel : class, new()
    {
        private readonly ISiteService _siteService;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public SiteSettingsPropertyDeploymentSource(
            ISiteService siteService,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _siteService = siteService;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            if (step is not SiteSettingsPropertyDeploymentStep<TModel> settingsStep)
            {
                return;
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();

            var settingJPropertyName = typeof(TModel).Name;
            var settingJPropertyValue = JObject.FromObject(siteSettings.As<TModel>(), _jsonSerializerOptions);

            var settingsStepJObject = result.Steps.FirstOrDefault(s => s["name"]?.ToString() == "Settings");
            if (settingsStepJObject != null)
            {
                settingsStepJObject.Add(settingJPropertyName, settingJPropertyValue);
            }
            else
            {
                result.Steps.Add(new JsonObject
                {
                    ["name"] = "Settings",
                    [settingJPropertyName] = settingJPropertyValue,
                });
            }
        }
    }
}
