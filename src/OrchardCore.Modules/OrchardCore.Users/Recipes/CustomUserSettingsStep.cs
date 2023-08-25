using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Users.Models;
using YesSql;

namespace OrchardCore.Users.Recipes
{
    /// <summary>
    /// This recipe step updates the custom user settings.
    /// </summary>
    public class CustomUserSettingsStep : IRecipeStepHandler
    {
        private ISession _session;

        public CustomUserSettingsStep(ISession session)
        {
            _session = session;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "custom-user-settings", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step;

            var customUserSettingsList = (JArray)(from property in model.Properties()
                                      where property.Name != "name"
                                      select property).FirstOrDefault().Value;

            var allUsers = (await _session.Query<User>().ListAsync());

            foreach (JObject userCustomUserSettings in customUserSettingsList)
            {
                var userId = userCustomUserSettings.Properties().FirstOrDefault(p => p.Name == "userId").Value.ToString();
                
                var iUser = allUsers.FirstOrDefault(u => u.UserId == userId);
                if (iUser == null)
                {
                    continue;
                }

                var user = iUser as User;
                var userSettings = (JArray)userCustomUserSettings.Properties().FirstOrDefault(p => p.Name == "user-custom-user-settings").Value;

                foreach (JObject userSetting in userSettings)
                {
                    var ci = userSetting.ToObject<ContentItem>();
                    user.Properties[ci.ContentType] = userSetting;
                    
                }
                
                _session.Save(user);
            }
        }
    }
}
