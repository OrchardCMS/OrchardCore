using System.Text.Json.Nodes;
using OrchardCore.CustomSettings.Services;
using OrchardCore.Deployment;

namespace OrchardCore.CustomSettings.Deployment;

public class CustomSettingsDeploymentSource
    : DeploymentSourceBase<CustomSettingsDeploymentStep>
{
    private readonly CustomSettingsService _customSettingsService;

    public CustomSettingsDeploymentSource(CustomSettingsService customSettingsService)
    {
        _customSettingsService = customSettingsService;
    }

    protected override async Task ProcessAsync(CustomSettingsDeploymentStep step, DeploymentPlanResult result)
    {
        var settingsList = new List<KeyValuePair<string, JsonNode>>
        {
            new("name", "custom-settings"),
        };

        var settingsTypes = step.IncludeAll
            ? (await _customSettingsService.GetAllSettingsTypesAsync()).ToArray()
            : (await _customSettingsService.GetSettingsTypesAsync(step.SettingsTypeNames)).ToArray();

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
            settingsList.Add(new(settings.ContentType, JObject.FromObject(settings)));
        }

        // Adding custom settings
        result.Steps.Add(new JsonObject(settingsList.ToArray()));
    }
}
