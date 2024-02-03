using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.Entities;

namespace OrchardCore.Settings.Deployment
{
    public class SiteSettingsPropertyDeploymentSource<TModel> : IDeploymentSource where TModel : class, new()
    {
        private readonly ISiteService _siteService;

        public SiteSettingsPropertyDeploymentSource(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var settingsStep = step as SiteSettingsPropertyDeploymentStep<TModel>;
            if (settingsStep == null)
            {
                return;
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();

            var settingJPropertyName = typeof(TModel).Name;
            var settingJPropertyValue = JObject.FromObject(siteSettings.As<TModel>());

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
