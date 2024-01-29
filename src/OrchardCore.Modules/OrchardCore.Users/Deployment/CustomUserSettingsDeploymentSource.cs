using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using YesSql;

namespace OrchardCore.Users.Deployment;

public class CustomUserSettingsDeploymentSource : IDeploymentSource
{
    private readonly CustomUserSettingsService _customUserSettingsService;
    private readonly ISession _session;

    public CustomUserSettingsDeploymentSource(CustomUserSettingsService customUserSettingsService, ISession session)
    {
        _customUserSettingsService = customUserSettingsService;
        _session = session;
    }

    public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        var customUserSettingsStep = step as CustomUserSettingsDeploymentStep;
        if (customUserSettingsStep == null)
        {
            return;
        }

        var settingsTypes = customUserSettingsStep.IncludeAll
            ? (await _customUserSettingsService.GetAllSettingsTypesAsync()).ToList()
            : (await _customUserSettingsService.GetSettingsTypesAsync(customUserSettingsStep.SettingsTypeNames)).ToList();

        // Todo: check permissions for each settings type
        var userData = new JArray();
        var allUsers = await _session.Query<User>().ListAsync();

        foreach (var user in allUsers)
        {
            var userSettingsData = new JArray();
            foreach (var settingsType in settingsTypes)
            {
                var userSetting = await _customUserSettingsService.GetSettingsAsync(user, settingsType);
                userSettingsData.Add(JObject.FromObject(userSetting));
            }

            userData.Add(new JObject(
                new JProperty("userId", user.UserId),
                new JProperty("user-custom-user-settings", userSettingsData)));
        }

        // Adding custom user settings
        result.Steps.Add(new JObject(
            new JProperty("name", "custom-user-settings"),
            new JProperty("custom-user-settings", userData)));
    }
}
