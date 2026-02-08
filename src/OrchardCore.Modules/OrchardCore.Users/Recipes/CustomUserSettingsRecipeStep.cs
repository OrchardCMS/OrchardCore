using System.Text.Json.Nodes;
using OrchardCore.Recipes.Schema;
using OrchardCore.ContentManagement;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Users.Models;
using YesSql;

namespace OrchardCore.Users.Recipes;

public sealed class CustomUserSettingsRecipeStep : RecipeImportStep<object>
{
    private readonly ISession _session;

    public CustomUserSettingsRecipeStep(ISession session)
    {
        _session = session;
    }

    public override string Name => "custom-user-settings";

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("Custom User Settings")
            .Description("Updates custom user settings for individual users.")
            .Required("name")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")))
            .AdditionalProperties(true)
            .Build();
    }

    protected override async Task ImportAsync(object model, RecipeExecutionContext context)
    {
        var step = context.Step;

        var customUserSettingsList = (JsonArray)step
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
                var contentItem = userSetting.ToObject<ContentItem>();
                user.Properties[contentItem.ContentType] = userSetting.DeepClone();
            }

            await _session.SaveAsync(user);
        }
    }
}
