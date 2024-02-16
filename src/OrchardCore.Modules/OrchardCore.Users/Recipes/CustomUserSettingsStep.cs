using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
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
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public CustomUserSettingsStep(
        ISession session,
        IOptions<JsonSerializerOptions> jsonSerializerOptions)
    {
        _session = session;
        _jsonSerializerOptions = jsonSerializerOptions.Value;
    }

    public async Task ExecuteAsync(RecipeExecutionContext context)
    {
        if (!string.Equals(context.Name, "custom-user-settings", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var model = context.Step;

        var customUserSettingsList = (JsonArray)model
            .AsEnumerable()
            .Where(p => p.Key != "name")
            .Select(p => p.Value)
            .FirstOrDefault();

        var allUsers = await _session.Query<User>().ListAsync();

        foreach (var userCustomUserSettings in customUserSettingsList.Cast<JsonObject>())
        {
            if (!userCustomUserSettings.TryGetPropertyValue("userId", out var jsonNode))
            {
                continue;
            }

            var userId = jsonNode.Value<string>();

            var user = allUsers.FirstOrDefault(u => u.UserId == userId);
            if (user is null)
            {
                continue;
            }

            if (!userCustomUserSettings.TryGetPropertyValue("user-custom-user-settings", out jsonNode) ||
                jsonNode is not JsonArray userSettings)
            {
                continue;
            }

            foreach (var userSetting in userSettings.Cast<JsonObject>())
            {
                var contentItem = userSetting.ToObject<ContentItem>(_jsonSerializerOptions);
                user.Properties[contentItem.ContentType] = userSetting;
            }

            await _session.SaveAsync(user);
        }
    }
}
