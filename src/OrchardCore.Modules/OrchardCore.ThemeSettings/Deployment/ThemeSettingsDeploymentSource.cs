using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Settings;
using OrchardCore.ThemeSettings.Services;

namespace OrchardCore.ThemeSettings.Deployment
{
    public class ThemeSettingsDeploymentSource : IDeploymentSource
    {
        private readonly IThemeSettingsService _themeSettingsService;
        private readonly ISiteService _siteService;

        public ThemeSettingsDeploymentSource(IThemeSettingsService themeSettingsService, ISiteService siteService)
        {
            _themeSettingsService = themeSettingsService;
            _siteService = siteService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var themeSettingsStep = step as ThemeSettingsDeploymentStep;

            if (themeSettingsStep == null)
            {
                return;
            }

            var themeSettings = await _themeSettingsService.GetThemeSettingsAsync();

            result.Steps.Add(new JObject(
                new JProperty("name", "Settings"),
                new JProperty("ThemeSettings", JObject.FromObject(themeSettings))
            ));
        }
    }
}
