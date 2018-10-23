using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.CustomSettings.Services;
using OrchardCore.Deployment;
using OrchardCore.Settings;

namespace OrchardCore.CustomSettings.Deployment
{
    public class CustomSettingsDeploymentSource : IDeploymentSource
    {
        private readonly ISiteService _siteService;
        private readonly CustomSettingsService _customSettingsService;

        public CustomSettingsDeploymentSource(
            ISiteService siteService,
            CustomSettingsService customSettingsService)
        {
            _siteService = siteService;
            _customSettingsService = customSettingsService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var customSettingsStep = step as CustomSettingsDeploymentStep;
            if (customSettingsStep == null)
            {
                return;
            }

            var settingsList = new List<JProperty> { new JProperty("name", "Settings") };

            var settingsTypes = customSettingsStep.IncludeAll
                ? _customSettingsService.GetAllSettingsTypes()
                : _customSettingsService.GetSettingsTypes(customSettingsStep.SettingsTypeNames);

            var settingsPermissionsTasks =
                from settingsType in settingsTypes
                select _customSettingsService.CanUserCreateSettingsAsync(settingsType);

            await Task.WhenAll(settingsPermissionsTasks);

            if (settingsPermissionsTasks.Any(t => !t.Result))
            {
                return;
            }

            var settingsTasks = from settingsType in settingsTypes
                                select _customSettingsService.GetSettingsAsync(settingsType);

            await Task.WhenAll(settingsTasks);

            settingsList.AddRange(
                from settingsTask in settingsTasks
                let settings = settingsTask.Result
                select new JProperty(settings.ContentType, JObject.FromObject(settings.Content)));

            // Adding custom settings
            result.Steps.Add(new JObject(settingsList.ToArray()));
        }
    }
}