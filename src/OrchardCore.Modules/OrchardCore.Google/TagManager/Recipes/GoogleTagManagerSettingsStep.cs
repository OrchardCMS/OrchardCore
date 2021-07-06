using System;
using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.Google.TagManager.Settings;
using OrchardCore.Google.TagManager.ViewModels;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Settings;

namespace OrchardCore.Google.TagManager.Recipes
{
    /// <summary>
    /// This recipe step sets Google TagManager settings.
    /// </summary>
    public class GoogleTagManagerSettingsStep : IRecipeStepHandler
    {
        private readonly ISiteService _siteService;
        public GoogleTagManagerSettingsStep(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, nameof(GoogleTagManagerSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            var model = context.Step.ToObject<GoogleTagManagerSettingsViewModel>();
            var container = await _siteService.LoadSiteSettingsAsync();
            container.Alter<GoogleTagManagerSettings>(nameof(GoogleTagManagerSettings), aspect =>
            {
                aspect.ContainerID = model.ContainerID;
            });
            await _siteService.UpdateSiteSettingsAsync(container);
        }
    }
}
