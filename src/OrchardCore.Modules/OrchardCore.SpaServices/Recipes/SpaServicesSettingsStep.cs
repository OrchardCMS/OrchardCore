using System;
using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.SpaServices.Settings;
using OrchardCore.SpaServices.ViewModels;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Settings;


namespace OrchardCore.SpaServices.Recipes
{
    /// <summary>
    /// This recipe step sets Google Analytics settings.
    /// </summary>
    public class SpaServicesSettingsStep : IRecipeStepHandler
    {
        readonly ISiteService _siteService;
        public SpaServicesSettingsStep(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, nameof(SpaServicesSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            var model = context.Step.ToObject<SpaServicesSettingsViewModel>();
            var site = await _siteService.GetSiteSettingsAsync();
            site.Alter<SpaServicesSettings>(nameof(SpaServicesSettings), aspect =>
            {
                aspect.UseStaticFile = model.UseStaticFile;
                aspect.StaticFile = model.StaticFile;
            });
            await _siteService.UpdateSiteSettingsAsync(site);
        }
    }
}