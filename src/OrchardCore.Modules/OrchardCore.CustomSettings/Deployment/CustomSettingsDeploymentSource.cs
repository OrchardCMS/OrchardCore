using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.CustomSettings.Services;
using OrchardCore.Deployment;

namespace OrchardCore.CustomSettings.Deployment
{
    public class CustomSettingsDeploymentSource : IDeploymentSource
    {
        private readonly CustomSettingsService _customSettingsService;

        public CustomSettingsDeploymentSource(CustomSettingsService customSettingsService)
        {
            _customSettingsService = customSettingsService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var customSettingsStep = step as CustomSettingsDeploymentStep;
            if (customSettingsStep == null)
            {
                return;
            }

            var settingsList = new List<JProperty> { new JProperty("name", "custom-settings") };

            var settingsTypes = customSettingsStep.IncludeAll
                ? _customSettingsService.GetAllSettingsTypes().ToArray()
                : _customSettingsService.GetSettingsTypes(customSettingsStep.SettingsTypeNames).ToArray();

            foreach (var settingsType in settingsTypes)
            {
                if (!await _customSettingsService.CanUserCreateSettingsAsync(settingsType))
                {
                    return;
                }
            }

            foreach (var settingsType in settingsTypes)
            {
                var settings = await _customSettingsService.GetSettingsAsync(settingsType);
                settingsList.Add(new JProperty(settings.ContentType, JObject.FromObject(settings)));
            }

            // Adding custom settings
            result.Steps.Add(new JObject(settingsList.ToArray()));
        }
    }
}
