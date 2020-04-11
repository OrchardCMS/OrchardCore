using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.CustomSettings.Services;
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
        private readonly CustomSettingsService _customSettingsService;

        public CustomSettingsStep(
            ISiteService siteService,
            CustomSettingsService customSettingsService)
        {
            _siteService = siteService;
            _customSettingsService = customSettingsService;
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

            var customSettingsNames = (from customSettings in customSettingsList
                                       select customSettings.Name).ToArray();

            var customSettingsTypes = _customSettingsService.GetSettingsTypes(customSettingsNames).ToArray();

            foreach (var customSettingsType in customSettingsTypes)
            {
                if (!await _customSettingsService.CanUserCreateSettingsAsync(customSettingsType))
                {
                    return;
                }
            }

            var siteSettings = await _siteService.LoadSiteSettingsAsync();

            foreach (var customSettings in customSettingsList)
            {
                siteSettings.Properties[customSettings.Name] = customSettings.Value;
            }

            await _siteService.UpdateSiteSettingsAsync(siteSettings);
        }
    }
}
