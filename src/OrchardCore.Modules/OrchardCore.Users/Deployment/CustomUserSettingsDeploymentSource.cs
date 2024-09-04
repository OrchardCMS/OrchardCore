using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using YesSql;

namespace OrchardCore.Users.Deployment;

public class CustomUserSettingsDeploymentSource : IDeploymentSource
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

    public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        if (step is not CustomUserSettingsDeploymentStep customUserSettingsStep)
        {
            return;
        }

        var settingsTypes = customUserSettingsStep.IncludeAll
            ? (await _customUserSettingsService.GetAllSettingsTypesAsync()).ToArray()
            : (await _customUserSettingsService.GetSettingsTypesAsync(customUserSettingsStep.SettingsTypeNames)).ToArray();

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
