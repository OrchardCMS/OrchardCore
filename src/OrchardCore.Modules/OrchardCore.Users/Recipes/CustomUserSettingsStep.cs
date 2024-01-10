using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Users.Models;
using YesSql;

namespace OrchardCore.Users.Recipes;

/// <summary>
/// This recipe step updates the custom user settings.
/// </summary>
public class CustomUserSettingsStep : IRecipeStepHandler
{
    private readonly ISession _session;

    public CustomUserSettingsStep(ISession session)
    {
        _session = session;
    }

    public async Task ExecuteAsync(RecipeExecutionContext context)
    {
        if (!string.Equals(context.Name, "custom-user-settings", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var model = context.Step;

        var customUserSettingsList = (JArray)model
            .Properties()
            .Where(p => p.Name != "name")
            .FirstOrDefault()
            ?.Value;

        var allUsers = await _session.Query<User>().ListAsync();

        foreach (JObject userCustomUserSettings in customUserSettingsList.Cast<JObject>())
        {
            var userId = userCustomUserSettings
                .Properties()
                .FirstOrDefault(p => p.Name == "userId")?
                .Value
                ?.ToString();

            var user = allUsers.FirstOrDefault(u => u.UserId == userId);
            if (user is not User _)
            {
                continue;
            }

            var userSettings = (JArray)userCustomUserSettings
                .Properties()
                .FirstOrDefault(p => p.Name == "user-custom-user-settings")
                ?.Value;

            foreach (JObject userSetting in userSettings.Cast<JObject>())
            {
                var contentItem = userSetting.ToObject<ContentItem>();
                user.Properties[contentItem.ContentType] = userSetting;
            }

            await _session.SaveAsync(user);
        }
    }
}
