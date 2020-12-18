using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Settings;

namespace OrchardCore.CustomSettings.Recipes
{
    /// <summary>
    /// This recipe step updates the site settings.
    /// </summary>
    public class CustomSettingsStep : IRecipeStepHandler
    {
        private readonly ISiteService _siteService;

        public CustomSettingsStep(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "custom-settings", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step;

            var customSettingsList = (from property in model.Properties()
                                      where property.Name != "name"
                                      select property).ToArray();

            var siteSettings = await _siteService.LoadSiteSettingsAsync();

            foreach (var customSettings in customSettingsList)
            {
                siteSettings.Properties[customSettings.Name] = customSettings.Value;
            }

            await _siteService.UpdateSiteSettingsAsync(siteSettings);
        }
    }
}
