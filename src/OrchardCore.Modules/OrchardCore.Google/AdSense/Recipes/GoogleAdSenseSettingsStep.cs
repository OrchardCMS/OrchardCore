using System;
using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.Google.AdSense.Settings;
using OrchardCore.Google.AdSense.ViewModels;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Settings;

namespace OrchardCore.Google.AdSense.Recipes
{
    public class GoogleAdSenseSettingsStep : IRecipeStepHandler
    {
        private readonly ISiteService _siteService;

        public GoogleAdSenseSettingsStep(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, nameof(GoogleAdSenseSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            var model = context.Step.ToObject<GoogleAdSenseSettingsViewModel>();
            var container = await _siteService.LoadSiteSettingsAsync();
            container.Alter<GoogleAdSenseSettings>(nameof(GoogleAdSenseSettings), aspect =>
            {
                aspect.PublisherID = model.PublisherID;
            });
            await _siteService.UpdateSiteSettingsAsync(container);
        }
    }
}