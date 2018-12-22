using System;
using System.Threading.Tasks;
using OrchardCore.Google.Services;
using OrchardCore.Google.Settings;
using OrchardCore.Google.ViewModels;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Google.Recipes
{
    /// <summary>
    /// This recipe step sets Microsoft Account settings.
    /// </summary>
    public class GoogleAuthenticationSettingsStep : IRecipeStepHandler
    {
        private readonly GoogleAuthenticationService _googleAuthenticationService;

        public GoogleAuthenticationSettingsStep(GoogleAuthenticationService googleAuthenticationService)
        {
            _googleAuthenticationService = googleAuthenticationService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, nameof(GoogleAuthenticationSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            var model = context.Step.ToObject<GoogleAuthenticationSettingsViewModel>();
            var settings = await _googleAuthenticationService.GetSettingsAsync();
            settings.ClientID = model.ClientID;
            settings.ClientSecret = model.ClientSecret;
            settings.CallbackPath = model.CallbackPath;
            await _googleAuthenticationService.UpdateSettingsAsync(settings);
        }
    }
}