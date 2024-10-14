using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using YesSql;

namespace OrchardCore.Users.Deployment;

public class CustomUserSettingsDeploymentSource
    : DeploymentSourceBase<CustomUserSettingsDeploymentStep>
{
    private readonly CustomUserSettingsService _customUserSettingsService;
    private readonly ISession _session;

    public CustomUserSettingsDeploymentSource(
        CustomUserSettingsService customUserSettingsService,
        ISession session)
    {
        _customUserSettingsService = customUserSettingsService;
        _session = session;
    }

    protected override async Task ProcessAsync(CustomUserSettingsDeploymentStep step, DeploymentPlanResult result)
    {
        var settingsTypes = step.IncludeAll
            ? (await _customUserSettingsService.GetAllSettingsTypesAsync()).ToArray()
            : (await _customUserSettingsService.GetSettingsTypesAsync(step.SettingsTypeNames)).ToArray();

        // Todo: check permissions for each settings type
        var userData = new JsonArray();
        var allUsers = await _session.Query<User>().ListAsync();

        foreach (var user in allUsers)
        {
            var userSettingsData = new JsonArray();
            foreach (var settingsType in settingsTypes)
            {
                var userSetting = await _customUserSettingsService.GetSettingsAsync(user, settingsType);
                userSettingsData.Add(JObject.FromObject(userSetting));
            }

            userData.Add(new JsonObject
            {
                ["userId"] = user.UserId,
                ["user-custom-user-settings"] = userSettingsData,
            });
        }

        // Adding custom user settings
        result.Steps.Add(new JsonObject
        {
            ["name"] = "custom-user-settings",
            ["custom-user-settings"] = userData,
        });
    }
}
