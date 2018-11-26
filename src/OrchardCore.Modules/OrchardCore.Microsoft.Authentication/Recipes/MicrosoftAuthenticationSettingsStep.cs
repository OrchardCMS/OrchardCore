using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using OrchardCore.Facebook.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using static OrchardCore.Facebook.Settings.AzureADAuthenticationSettings;

namespace OrchardCore.Facebook.Recipes
{
    /// <summary>
    /// This recipe step sets general OpenID Connect Client settings.
    /// </summary>
    public class MicrosoftAuthenticationSettingsStep : IRecipeStepHandler
    {
        private readonly IMicrosoftAuthenticationService _loginService;

        public MicrosoftAuthenticationSettingsStep(IMicrosoftAuthenticationService loginService)
        {
            _loginService = loginService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "FacebookLoginSettings", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<FacebookLoginSettingsStepModel>();

            var settings = await _loginService.GetSettingsAsync();
            settings.CallbackPath = model.CallbackPath;

            await _loginService.UpdateSettingsAsync(settings);
        }
    }

    public class FacebookLoginSettingsStepModel
    {
        public string CallbackPath { get; set; }
    }
}