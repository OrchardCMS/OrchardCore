using System;
using System.Threading.Tasks;
using OrchardCore.Facebook.Login.Services;
using OrchardCore.Facebook.Login.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.OpenId.Recipes
{
    /// <summary>
    /// This recipe step sets general OpenID Connect settings.
    /// </summary>
    public class FacebookLoginSettingsStep : IRecipeStepHandler
    {
        private readonly IFacebookLoginService _facebookLoginService;

        public FacebookLoginSettingsStep(IFacebookLoginService facebookLoginService)
            => _facebookLoginService = facebookLoginService;

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, nameof(FacebookLoginSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<FacebookLoginSettingsStepModel>();
            var settings = await _facebookLoginService.LoadSettingsAsync();

            settings.CallbackPath = model.CallbackPath;

            await _facebookLoginService.UpdateSettingsAsync(settings);
        }
    }

    public class FacebookLoginSettingsStepModel
    {
        public string CallbackPath { get; set; }
    }
}
