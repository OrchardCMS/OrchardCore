using System;
using System.Threading.Tasks;
using OrchardCore.Github.Services;
using OrchardCore.Github.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Github.Recipes
{
    /// <summary>
    /// This recipe step sets Microsoft Account settings.
    /// </summary>
    public class GithubSigninSettingsStep : IRecipeStepHandler
    {
        private readonly IGithubAuthenticationService _twitterLoginService;

        public GithubSigninSettingsStep(IGithubAuthenticationService twitterLoginService)
        {
            _twitterLoginService = twitterLoginService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, nameof(GithubAuthenticationSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            var model = context.Step.ToObject<GithubLoginSettingsStepModel>();
            var settings = await _twitterLoginService.GetSettingsAsync();
            settings.ClientID = model.ConsumerKey;
            settings.ClientSecret = model.ConsumerSecret;
            settings.CallbackPath = model.CallbackPath;
            await _twitterLoginService.UpdateSettingsAsync(settings);
        }
    }

    public class GithubLoginSettingsStepModel
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string CallbackPath { get; set; }
    }
}