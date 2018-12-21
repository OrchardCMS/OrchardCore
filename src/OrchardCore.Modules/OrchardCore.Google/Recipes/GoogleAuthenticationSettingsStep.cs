using System;
using System.Threading.Tasks;
using OrchardCore.Google.Services;
using OrchardCore.Google.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Google.Recipes
{
    /// <summary>
    /// This recipe step sets Microsoft Account settings.
    /// </summary>
    public class GoogleAuthenticationSettingsStep : IRecipeStepHandler
    {
        private readonly IGoogleAuthenticationService _twitterLoginService;

        public GoogleAuthenticationSettingsStep(IGoogleAuthenticationService twitterLoginService)
        {
            _twitterLoginService = twitterLoginService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, nameof(GoogleAuthenticationSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            var model = context.Step.ToObject<TwitterLoginSettingsStepModel>();
            var settings = await _twitterLoginService.GetSettingsAsync();
            settings.ClientID = model.ConsumerKey;
            settings.ClientSecret = model.ConsumerSecret;
            settings.CallbackPath = model.CallbackPath;
            await _twitterLoginService.UpdateSettingsAsync(settings);
        }
    }

    public class TwitterLoginSettingsStepModel
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string CallbackPath { get; set; }
    }
}